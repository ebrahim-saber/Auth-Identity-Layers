using System.Reflection;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using IdentityAuthLayer.API.Middlewares;
using IdentityAuthLayer.Application.Common.Interfaces;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Application.Services;
using IdentityAuthLayer.Infrastructure.Email;
using IdentityAuthLayer.Infrastructure.Identity;
using IdentityAuthLayer.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------- SQL Server + EF Core ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Unit of Work (بيلف كل الـ Repositories) ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---------- Application Services (الـ Business Logic) ----------
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ISystemService, SystemService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ---------- Email (SendGrid) ----------
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// ---------- MediatR (CQRS) ----------
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("IdentityAuthLayer.Application")));

// ---------- FluentValidation ----------
builder.Services.AddValidatorsFromAssembly(Assembly.Load("IdentityAuthLayer.Application"));

// بيشغّل الـ Validators فعليًا قبل كل Handler (من غيره الـ Validators متسجلة بس ملهاش تأثير)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdentityAuthLayer.Application.Common.Behaviors.ValidationBehavior<,>));

// ---------- Identity services (Password hashing + JWT) ----------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.Configure<AuthSecurityOptions>(builder.Configuration.GetSection("AuthSecurity"));
builder.Services.AddSingleton<IAuthSecurity, AuthSecurity>();

// ---------- JWT Authentication (نفس التوكن ده هيتحقق منه أي سيستم فرعي تاني) ----------
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings غير موجودة في appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ---------- Authorization Policies ----------
// AdminOnly: للـ endpoints الإدارية (تسجيل عملاء/أنظمة/رولز/برميشنز/يوزرز) - لازم الـ claim "userType" = "Admin"
// أي endpoint مش عليه [AllowAnonymous] أو [Authorize(Policy=...)] بيتطلب توكن صالح بس (بسبب الـ AuthorizeFilter تحت)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("userType", "Admin"));
    options.AddPolicy("AdminOrGroupAdmin", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("userType", "Admin") || ctx.User.HasClaim("userType", "GroupAdmin")));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

// ---------- Controllers ----------
builder.Services.AddControllers();

// ---------- Swagger + JWT ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity & Authorization API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "ضع رمز التوكن (JWT Token) هنا مباشرة بدون كلمة Bearer:"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- CORS ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
    {
        if (allowedOrigins.Length == 0) p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity & Authorization API V1");
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------- Auto-migrate on startup ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
