using eixample.Consts;
using eixample.Entities;
using System;
using System.Linq;
using Xunit;

namespace eixample.Tests.Integration
{
    public class EntityAudit_Tests : TestBase
    {
        [Fact]
        public void Should_Set_Audit_Properties()
        {
            var tenant = DbContext.Tenants.Single(x => x.HostName == SetupConsts.Tenants.Subdomain1.HostName);
            var adminJoe = DbContext.Users.Single(x => x.UserName == SetupConsts.Users.AdminJoe.UserName);
            var johnRoe = DbContext.Users.Single(x => x.UserName == SetupConsts.Users.JohnRoe.UserName);

            // Admin Joe creates team
            DbContext.UserId = adminJoe.Id;
            DbContext.TenantId = tenant.Id;
            var team = new Person() { Name = "Team1", Description = "random text..." };
            DbContext.Persons.Add(team);
            DbContext.SaveChanges();

            // John Roe modifies team
            DbContext.UserId = johnRoe.Id;
            team.Description = "text changes...";
            DbContext.SaveChanges();

            // Admin Joe deletes team
            DbContext.UserId = adminJoe.Id;
            DbContext.Persons.Remove(team);
            DbContext.SaveChanges();

            var moment = DateTime.UtcNow;

            // testing creation audit
            Assert.Equal(team.TenantId, tenant.Id);
            Assert.Equal(team.CreatorId, adminJoe.Id);
            Assert.True(team.CreationTime > moment.AddSeconds(-10) && team.CreationTime < moment.AddSeconds(10));

            // testing modification audit
            Assert.Equal(team.ModifierId, johnRoe.Id);
            Assert.True(team.ModificationTime > moment.AddSeconds(-10) && team.ModificationTime < moment.AddSeconds(10));

            // testing deletion audit
            Assert.Equal(team.DeleterId, adminJoe.Id);
            Assert.True(team.DeletionTime > moment.AddSeconds(-10) && team.DeletionTime < moment.AddSeconds(10));
        }
    }
}