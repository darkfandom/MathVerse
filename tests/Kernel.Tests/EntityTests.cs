using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
        public TestEntity() : base(Guid.NewGuid()) { }
    }

    private sealed class TestEntityWithInt : Entity<int>
    {
        public TestEntityWithInt(int id) : base(id) { }
    }

    [Fact]
    public void Entity_HasId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Entity_GeneratesId_WhenParameterless()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Entity_VersionStartsAtZero()
    {
        var entity = new TestEntity();

        entity.Version.Should().Be(0);
    }

    [Fact]
    public void Entity_EqualById()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.Should().Be(entity2);
    }

    [Fact]
    public void Entity_NotEqualByDifferentId()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        entity1.Should().NotBe(entity2);
    }

    [Fact]
    public void Entity_EqualityOperators()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);
        TestEntity? nullEntity = null;

        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
        (entity1 == nullEntity).Should().BeFalse();
        (nullEntity != entity1).Should().BeTrue();
    }

    [Fact]
    public void Entity_WithIntId()
    {
        var entity = new TestEntityWithInt(42);

        entity.Id.Should().Be(42);
    }

    [Fact]
    public void Entity_DifferentTypes_WithSameId_NotEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntityDifferent(id);

        entity1.Should().NotBe(entity2);
    }

    private sealed class TestEntityDifferent : Entity<Guid>
    {
        public TestEntityDifferent(Guid id) : base(id) { }
    }
}
