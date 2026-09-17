using CoParenting.Domain.Entities;

namespace CoParenting.UnitTests;

public class FamilyTests
{
    [Fact]
    public void NewFamily_HasNoMembersByDefault()
    {
        var family = new Family { Id = Guid.NewGuid(), Name = "Família Teste" };

        Assert.Empty(family.Members);
    }
}
