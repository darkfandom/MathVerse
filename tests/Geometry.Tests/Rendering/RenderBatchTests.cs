using System.Collections.Immutable;

namespace MathVerse.Geometry.Tests.Rendering;

/// <summary>Tests for the <see cref="RenderBatch"/> class.</summary>
public class RenderBatchTests
{
    /// <summary>Verifies that AddCommand increments the command count.</summary>
    [Fact]
    public void AddCommand_IncrementsCommandCount()
    {
        var batch = new RenderBatch();
        var cmd = new RenderCommand { MaterialName = "mat1" };

        batch.AddCommand(cmd);

        batch.CommandCount.Should().Be(1);
    }

    /// <summary>Verifies that AddCommand with different materials increases material count.</summary>
    [Fact]
    public void AddCommand_DifferentMaterial_IncrementsMaterialCount()
    {
        var batch = new RenderBatch();
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });
        batch.AddCommand(new RenderCommand { MaterialName = "mat2" });

        batch.MaterialCount.Should().Be(2);
    }

    /// <summary>Verifies that AddCommand with same material does not increase material count.</summary>
    [Fact]
    public void AddCommand_SameMaterial_SameMaterialCount()
    {
        var batch = new RenderBatch();
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });

        batch.MaterialCount.Should().Be(1);
    }

    /// <summary>Verifies that GetGroupedCommands groups commands by material name.</summary>
    [Fact]
    public void GetGroupedCommands_GroupsByMaterial()
    {
        var batch = new RenderBatch();
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });
        batch.AddCommand(new RenderCommand { MaterialName = "mat2" });

        var grouped = batch.GetGroupedCommands();

        grouped.Should().ContainKey("mat1");
        grouped.Should().ContainKey("mat2");
        grouped["mat1"].Should().HaveCount(2);
        grouped["mat2"].Should().HaveCount(1);
    }

    /// <summary>Verifies that CommandCount returns total number of commands across all materials.</summary>
    [Fact]
    public void CommandCount_ReturnsTotal()
    {
        var batch = new RenderBatch();
        batch.AddCommand(new RenderCommand { MaterialName = "a" });
        batch.AddCommand(new RenderCommand { MaterialName = "a" });
        batch.AddCommand(new RenderCommand { MaterialName = "b" });
        batch.AddCommand(new RenderCommand { MaterialName = "c" });

        batch.CommandCount.Should().Be(4);
    }

    /// <summary>Verifies that MaterialCount returns zero for an empty batch.</summary>
    [Fact]
    public void MaterialCount_EmptyBatch_ReturnsZero()
    {
        var batch = new RenderBatch();

        batch.MaterialCount.Should().Be(0);
    }

    /// <summary>Verifies that CommandCount returns zero for an empty batch.</summary>
    [Fact]
    public void CommandCount_EmptyBatch_ReturnsZero()
    {
        var batch = new RenderBatch();

        batch.CommandCount.Should().Be(0);
    }

    /// <summary>Verifies that Clear removes all commands.</summary>
    [Fact]
    public void Clear_RemovesAllCommands()
    {
        var batch = new RenderBatch();
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });
        batch.AddCommand(new RenderCommand { MaterialName = "mat2" });

        batch.Clear();

        batch.CommandCount.Should().Be(0);
        batch.MaterialCount.Should().Be(0);
    }

    /// <summary>Verifies that Clear followed by AddCommand works correctly.</summary>
    [Fact]
    public void Clear_ThenAdd_WorksCorrectly()
    {
        var batch = new RenderBatch();
        batch.AddCommand(new RenderCommand { MaterialName = "mat1" });

        batch.Clear();
        batch.AddCommand(new RenderCommand { MaterialName = "mat2" });

        batch.CommandCount.Should().Be(1);
        batch.MaterialCount.Should().Be(1);
    }

    /// <summary>Verifies that GetGroupedCommands returns empty dictionary for empty batch.</summary>
    [Fact]
    public void GetGroupedCommands_EmptyBatch_ReturnsEmpty()
    {
        var batch = new RenderBatch();

        var grouped = batch.GetGroupedCommands();

        grouped.Should().BeEmpty();
    }
}
