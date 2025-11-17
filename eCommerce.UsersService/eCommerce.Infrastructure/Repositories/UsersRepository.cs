using Dapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Infrastructure.DbContext;

namespace eCommerce.Infrastructure.Repositories;

internal sealed class UsersRepository : IUsersRepository
{
    private readonly DapperDbContext _dbContext;

    public UsersRepository(DapperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationUser?> AddUser(ApplicationUser user)
    {
        user.UserId = Guid.NewGuid();

        const string query = """
                             INSERT INTO public."Users" ("user_id", "email", "person_name", "gender", "password")
                             VALUES (@UserId, @Email, @PersonName, @Gender, @Password);
                             """;

        int rowsCountAffected = await _dbContext.DbConnection.ExecuteAsync(query, user);

        return rowsCountAffected > 0 ? user : null;
    }

    public async Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password)
    {
        const string query = """
                             SELECT 
                                 user_id      AS "UserId",
                                 email        AS "Email",
                                 password     AS "Password",
                                 person_name  AS "PersonName",
                                 gender       AS "Gender"
                             FROM public."Users"
                             WHERE email = @Email AND password = @Password;
                             """;

        ApplicationUser? user = await _dbContext.DbConnection.QueryFirstOrDefaultAsync<ApplicationUser>(
            query, new { Email = email, Password = password });

        return user;
    }
}