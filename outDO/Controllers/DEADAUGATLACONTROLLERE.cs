/*
 * 
 * pas 10 user si roluri
 * using Microsoft.AspNetCore.Identity;
using outDO.Data;

private readonly ApplicationDbContext db;
private readonly UserManager<ApplicationUser> _userManager;
private readonly RoleManager<IdentityRole> _roleManager;
public ArticlesController(
ApplicationDbContext context,
UserManager<ApplicationUser> userManager,
RoleManager<IdentityRole> roleManager
)
{
    db = context;
    _userManager = userManager;
    _roleManager = roleManager;
}*/