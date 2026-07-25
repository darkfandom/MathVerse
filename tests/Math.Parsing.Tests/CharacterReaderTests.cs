namespace MathVerse.Math.Parsing.Tests;

public class CharacterReaderTests
{
    [Fact]
    public void Constructor_NullSource_ThrowsArgumentNullException()
    {
        var act = () => new CharacterReader(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Peek_AtStart_ReturnsFirstChar()
    {
        var reader = new CharacterReader("abc");
        reader.Peek().Should().Be('a');
    }

    [Fact]
    public void Peek_AtEnd_ReturnsNullChar()
    {
        var reader = new CharacterReader("a");
        reader.Advance();
        reader.Peek().Should().Be('\0');
    }

    [Fact]
    public void Peek_DoesNotAdvancePosition()
    {
        var reader = new CharacterReader("ab");
        reader.Peek();
        reader.Peek();
        reader.Position.Should().Be(0);
    }

    [Fact]
    public void Peek_WithOffset_ReturnsCorrectChar()
    {
        var reader = new CharacterReader("abc");
        reader.Peek(0).Should().Be('a');
        reader.Peek(1).Should().Be('b');
        reader.Peek(2).Should().Be('c');
    }

    [Fact]
    public void Peek_WithNegativeOffset_ReturnsNullChar()
    {
        var reader = new CharacterReader("abc");
        reader.Peek(-1).Should().Be('\0');
    }

    [Fact]
    public void Peek_WithOffsetBeyondLength_ReturnsNullChar()
    {
        var reader = new CharacterReader("ab");
        reader.Peek(5).Should().Be('\0');
    }

    [Fact]
    public void Read_AdvancesAndReturnsChar()
    {
        var reader = new CharacterReader("abc");
        reader.Read().Should().Be('a');
        reader.Position.Should().Be(1);
        reader.Read().Should().Be('b');
        reader.Position.Should().Be(2);
    }

    [Fact]
    public void Read_AtEnd_ReturnsNullChar()
    {
        var reader = new CharacterReader("a");
        reader.Advance();
        reader.Read().Should().Be('\0');
    }

    [Fact]
    public void ReadWithCount_ReturnsSubstring()
    {
        var reader = new CharacterReader("abcdef");
        reader.Read(3).Should().Be("abc");
        reader.Position.Should().Be(3);
    }

    [Fact]
    public void ReadWithCount_ExceedingLength_ReturnsPartial()
    {
        var reader = new CharacterReader("ab");
        reader.Read(5).Should().Be("ab");
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void ReadWithCount_Zero_ReturnsEmpty()
    {
        var reader = new CharacterReader("abc");
        reader.Read(0).Should().BeEmpty();
        reader.Position.Should().Be(0);
    }

    [Fact]
    public void Match_MatchingChar_ReturnsTrueAndAdvances()
    {
        var reader = new CharacterReader("ab");
        reader.Match('a').Should().BeTrue();
        reader.Position.Should().Be(1);
        reader.Peek().Should().Be('b');
    }

    [Fact]
    public void Match_NonMatchingChar_ReturnsFalseAndDoesNotAdvance()
    {
        var reader = new CharacterReader("abc");
        reader.Match('x').Should().BeFalse();
        reader.Position.Should().Be(0);
    }

    [Fact]
    public void Match_AtEnd_ReturnsFalse()
    {
        var reader = new CharacterReader("a");
        reader.Advance();
        reader.Match('a').Should().BeFalse();
    }

    [Fact]
    public void MatchAny_MatchingChar_ReturnsTrueAndAdvances()
    {
        var reader = new CharacterReader("abc");
        reader.MatchAny('x', 'a', 'y').Should().BeTrue();
        reader.Position.Should().Be(1);
    }

    [Fact]
    public void MatchAny_NoMatchingChar_ReturnsFalse()
    {
        var reader = new CharacterReader("abc");
        reader.MatchAny('x', 'y').Should().BeFalse();
        reader.Position.Should().Be(0);
    }

    [Fact]
    public void MatchAny_EmptySource_ReturnsFalse()
    {
        var reader = new CharacterReader("");
        reader.MatchAny('a').Should().BeFalse();
    }

    [Fact]
    public void ReadWhile_ReadsMatchingChars()
    {
        var reader = new CharacterReader("abc123");
        var result = reader.ReadWhile(char.IsLetter);
        result.Should().Be("abc");
        reader.Position.Should().Be(3);
    }

    [Fact]
    public void ReadWhile_NoMatch_ReturnsEmpty()
    {
        var reader = new CharacterReader("123");
        var result = reader.ReadWhile(char.IsLetter);
        result.Should().BeEmpty();
        reader.Position.Should().Be(0);
    }

    [Fact]
    public void ReadWhile_AllMatch_ReturnsFullString()
    {
        var reader = new CharacterReader("abc");
        var result = reader.ReadWhile(char.IsLetter);
        result.Should().Be("abc");
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void ReadWhile_EmptySource_ReturnsEmpty()
    {
        var reader = new CharacterReader("");
        var result = reader.ReadWhile(_ => true);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReadUntil_StopsWhenPredicateMatches()
    {
        var reader = new CharacterReader("abc123");
        var result = reader.ReadUntil(c => char.IsDigit(c));
        result.Should().Be("abc");
        reader.Position.Should().Be(3);
    }

    [Fact]
    public void ReadUntil_PredicateNeverMatches_ReadsAll()
    {
        var reader = new CharacterReader("abc");
        var result = reader.ReadUntil(c => c == 'x');
        result.Should().Be("abc");
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void ReadUntil_PredicateMatchesImmediately_ReturnsEmpty()
    {
        var reader = new CharacterReader("abc");
        var result = reader.ReadUntil(c => c == 'a');
        result.Should().BeEmpty();
        reader.Position.Should().Be(0);
    }

    [Fact]
    public void IsAtEnd_EmptySource_ReturnsTrue()
    {
        var reader = new CharacterReader("");
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void IsAtEnd_StartOfNonEmptySource_ReturnsFalse()
    {
        var reader = new CharacterReader("a");
        reader.IsAtEnd.Should().BeFalse();
    }

    [Fact]
    public void IsAtEnd_AfterReadingAll_ReturnsTrue()
    {
        var reader = new CharacterReader("ab");
        reader.Advance();
        reader.Advance();
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void Advance_IncrementsColumn()
    {
        var reader = new CharacterReader("abc");
        reader.Line.Should().Be(1);
        reader.Column.Should().Be(1);
        reader.Advance();
        reader.Line.Should().Be(1);
        reader.Column.Should().Be(2);
    }

    [Fact]
    public void Advance_Newline_IncrementsLineAndResetsColumn()
    {
        var reader = new CharacterReader("a\nb");
        reader.Advance();
        reader.Line.Should().Be(1);
        reader.Column.Should().Be(2);
        reader.Advance();
        reader.Line.Should().Be(2);
        reader.Column.Should().Be(1);
    }

    [Fact]
    public void Advance_WithCount_AdvancesMultipleChars()
    {
        var reader = new CharacterReader("abcdef");
        reader.Advance(3);
        reader.Position.Should().Be(3);
        reader.Peek().Should().Be('d');
    }

    [Fact]
    public void Advance_AtEnd_DoesNotThrow()
    {
        var reader = new CharacterReader("a");
        reader.Advance();
        reader.Advance();
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void Advance_WithCount_BeyondEnd_StopsAtEnd()
    {
        var reader = new CharacterReader("ab");
        reader.Advance(10);
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void Advance_WithZeroCount_DoesNothing()
    {
        var reader = new CharacterReader("abc");
        reader.Advance(0);
        reader.Position.Should().Be(0);
        reader.Peek().Should().Be('a');
    }

    [Fact]
    public void GetSubstring_ReturnsCorrectSubstringWithEnd()
    {
        var reader = new CharacterReader("abcdef");
        reader.GetSubstring(0, 3).Should().Be("abc");
        reader.GetSubstring(2, 5).Should().Be("cde");
        reader.GetSubstring(0, 0).Should().BeEmpty();
    }

    [Fact]
    public void GetSubstring_WithNegativeStart_ClampsToZero()
    {
        var reader = new CharacterReader("abc");
        reader.GetSubstring(-5, 2).Should().Be("ab");
    }

    [Fact]
    public void GetSubstring_EmptySource_ReturnsEmpty()
    {
        var reader = new CharacterReader("");
        reader.GetSubstring(0).Should().BeEmpty();
    }

    [Fact]
    public void GetSubstring_StartGreaterThanEnd_ReturnsEmpty()
    {
        var reader = new CharacterReader("abc");
        reader.GetSubstring(5, 2).Should().BeEmpty();
    }

    [Fact]
    public void GetRemaining_ReturnsRestOfSource()
    {
        var reader = new CharacterReader("abcdef");
        reader.Advance(3);
        reader.GetRemaining().Should().Be("def");
    }

    [Fact]
    public void GetRemaining_AtEnd_ReturnsEmpty()
    {
        var reader = new CharacterReader("a");
        reader.Advance();
        reader.GetRemaining().Should().BeEmpty();
    }

    [Fact]
    public void GetRemaining_AtStart_ReturnsFullSource()
    {
        var reader = new CharacterReader("abc");
        reader.GetRemaining().Should().Be("abc");
    }

    [Fact]
    public void GetRemaining_EmptySource_ReturnsEmpty()
    {
        var reader = new CharacterReader("");
        reader.GetRemaining().Should().BeEmpty();
    }

    [Fact]
    public void Length_ReturnsSourceLength()
    {
        new CharacterReader("abc").Length.Should().Be(3);
        new CharacterReader("").Length.Should().Be(0);
        new CharacterReader("a").Length.Should().Be(1);
    }

    [Fact]
    public void EmptySource_AllPropertiesCorrect()
    {
        var reader = new CharacterReader("");
        reader.Length.Should().Be(0);
        reader.IsAtEnd.Should().BeTrue();
        reader.Position.Should().Be(0);
        reader.Line.Should().Be(1);
        reader.Column.Should().Be(1);
        reader.Peek().Should().Be('\0');
    }

    [Fact]
    public void SingleCharSource_ReadAndCheck()
    {
        var reader = new CharacterReader("x");
        reader.Peek().Should().Be('x');
        reader.Read().Should().Be('x');
        reader.IsAtEnd.Should().BeTrue();
    }

    [Fact]
    public void MultiLineSource_TracksLineNumbersCorrectly()
    {
        var reader = new CharacterReader("a\nb\nc");
        reader.Line.Should().Be(1);
        reader.Read();
        reader.Read();
        reader.Line.Should().Be(2);
        reader.Read();
        reader.Read();
        reader.Line.Should().Be(3);
    }

    [Fact]
    public void MultiLineSource_TracksColumnAfterNewlines()
    {
        var reader = new CharacterReader("ab\ncd");
        reader.Column.Should().Be(1);
        reader.Read();
        reader.Column.Should().Be(2);
        reader.Read();
        reader.Column.Should().Be(3);
        reader.Read();
        reader.Line.Should().Be(2);
        reader.Column.Should().Be(1);
        reader.Read();
        reader.Column.Should().Be(2);
    }

    [Fact]
    public void CurrentPosition_ReturnsCorrectTokenPosition()
    {
        var reader = new CharacterReader("abc");
        var pos = reader.CurrentPosition;
        pos.Line.Should().Be(1);
        pos.Column.Should().Be(1);
        pos.Offset.Should().Be(0);
        reader.Advance();
        pos = reader.CurrentPosition;
        pos.Line.Should().Be(1);
        pos.Column.Should().Be(2);
        pos.Offset.Should().Be(1);
    }

    [Fact]
    public void Read_Newline_ResetsColumnAndIncrementsLine()
    {
        var reader = new CharacterReader("ab\ncd");
        reader.Read();
        reader.Column.Should().Be(2);
        reader.Read();
        reader.Column.Should().Be(3);
        reader.Read();
        reader.Line.Should().Be(2);
        reader.Column.Should().Be(1);
    }

    [Fact]
    public void Advance_ToNewline_UpdatesLineAndColumn()
    {
        var reader = new CharacterReader("x\ny");
        reader.Advance(2);
        reader.Line.Should().Be(2);
        reader.Column.Should().Be(1);
    }

    [Fact]
    public void MultipleConsecutiveNewlines_TrackCorrectly()
    {
        var reader = new CharacterReader("\n\n\nx");
        reader.Line.Should().Be(1);
        reader.Advance();
        reader.Line.Should().Be(2);
        reader.Advance();
        reader.Line.Should().Be(3);
        reader.Advance();
        reader.Line.Should().Be(4);
        reader.Advance();
        reader.Column.Should().Be(2);
    }
}
