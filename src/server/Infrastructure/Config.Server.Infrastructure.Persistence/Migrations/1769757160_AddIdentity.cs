using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

#pragma warning disable SA1649

namespace Config.Server.Infrastructure.Persistence.Migrations;

[Migration(version: 1769757160, description: "Add Identity migration")]
public class AddIdentity : IMigration, IMigrationAssemblyMarker
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            create table identities
            (
                id            uuid primary key,
                
                username      text unique not null,
                password      text not null,
                email         text unique,
                refresh_token text unique,
                
                created_at    timestamp with time zone not null
            );
            """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            drop table identities;
            """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}