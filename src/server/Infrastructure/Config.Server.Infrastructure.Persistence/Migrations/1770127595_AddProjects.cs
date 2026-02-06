#pragma warning disable SA1649

using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Config.Server.Infrastructure.Persistence.Migrations;

[Migration(version: 1770127595, description: "Add Projects migration")]
public class AddProjects : IMigration, IMigrationAssemblyMarker
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            create table projects
            (
                id         uuid primary key,
                
                name       text unique not null,
                owner_id   uuid not null,
                
                created_at timestamp with time zone not null
            );
           
            create type project_roles as enum ('maintainer', 'editor', 'reader');
           
            create table project_members
            (
                project_id   uuid not null,
                user_id      uuid not null,

                role         project_roles not null,
                
                is_revoked   bool not null,
                
                created_at timestamp with time zone not null,
                
                constraint unique_project_member_pair
                    unique (project_id, user_id)
            );
            """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
            drop table projects;
            drop table project_members;
            drop type project_roles;
            """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}