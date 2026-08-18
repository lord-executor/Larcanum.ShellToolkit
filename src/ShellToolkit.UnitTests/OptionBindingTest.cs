using System.CommandLine;

using AwesomeAssertions;

using Larcanum.ShellToolkit.Terminal.Integration;

using Xunit;

namespace ShellToolkit.UnitTests;

public class OptionBindingTest
{
    private class TestArgWithDefault : IArguments<TestArgWithDefault>
    {
        public string NonNullableNotInitializedWithoutDefault { get; set; } = default!;
        public string NonNullableNotInitializedWithDefault { get; set; } = default!;
        public string NonNullableInitializedWithoutDefault { get; set; } = "initial";
        public string NonNullableInitializedWithDefault { get; set; } = "initial";
        public string? NullableNotInitializedWithoutDefault { get; set; }
        public string? NullableNotInitializedWithDefault { get; set; }
        public string? NullableInitializedWithoutDefault { get; set; } = "initial";
        public string? NullableInitializedWithDefault { get; set; } = "initial";

        public bool FlagWithoutDefault { get; set; }
        public bool FlagWithDefault { get; set; }
        public bool? NullableFlagWithoutDefault { get; set; }
        public bool? NullableFlagWithDefault { get; set; }

        public static IEnumerable<ISymbolBinding<TestArgWithDefault>> Register(BindingBuilder<TestArgWithDefault> builder)
        {
            yield return builder.BindOption(x => x.NonNullableNotInitializedWithoutDefault, "--nn-ni-wd", string.Empty);
            yield return builder.BindOption(x => x.NonNullableNotInitializedWithDefault, "--nn-ni-d", string.Empty)
                .WithDefaultValue("default");
            yield return builder.BindOption(x => x.NonNullableInitializedWithoutDefault, "--nn-i-wd", string.Empty);
            yield return builder.BindOption(x => x.NonNullableInitializedWithDefault, "--nn-i-d", string.Empty)
                .WithDefaultValue("default");
            yield return builder.BindOption(x => x.NullableNotInitializedWithoutDefault, "--n-ni-wd", string.Empty);
            yield return builder.BindOption(x => x.NullableNotInitializedWithDefault, "--n-ni-d", string.Empty)
                .WithDefaultValue("default");
            yield return builder.BindOption(x => x.NullableInitializedWithoutDefault, "--n-i-wd", string.Empty);
            yield return builder.BindOption(x => x.NullableInitializedWithDefault, "--n-i-d", string.Empty)
                .WithDefaultValue("default");

            yield return builder.BindOption(x => x.FlagWithoutDefault, "--f-wd", string.Empty);
            yield return builder.BindOption(x => x.FlagWithDefault, "--f-d", string.Empty)
                .WithDefaultValue(true);
            yield return builder.BindOption(x => x.NullableFlagWithoutDefault, "--nf-wd", string.Empty);
            yield return builder.BindOption(x => x.NullableFlagWithDefault, "--nf-d", string.Empty)
                .WithDefaultValue(true);
        }
    }

    [Fact]
    public void Options_InitialPropertyValue_AlwaysOverwrittenByOptionValue()
    {
        var command = new RootCommand();
        var factory = new ArgumentFactory<TestArgWithDefault>(command);

        var parseResult = command.Parse([]);
        var args = factory.Create(parseResult);

        args.NonNullableNotInitializedWithoutDefault.Should().BeNull(); // CAREFUL with this one!
        args.NonNullableNotInitializedWithDefault.Should().Be("default");
        args.NonNullableInitializedWithoutDefault.Should().BeNull();
        args.NonNullableInitializedWithDefault.Should().Be("default");
        args.NullableNotInitializedWithoutDefault.Should().BeNull();
        args.NullableNotInitializedWithDefault.Should().Be("default");
        args.NullableInitializedWithoutDefault.Should().BeNull();
        args.NullableInitializedWithDefault.Should().Be("default");

        args.FlagWithoutDefault.Should().BeFalse();
        args.FlagWithDefault.Should().BeTrue();
        args.NullableFlagWithoutDefault.Should().BeNull();
        args.NullableFlagWithDefault.Should().BeTrue();
    }
}
