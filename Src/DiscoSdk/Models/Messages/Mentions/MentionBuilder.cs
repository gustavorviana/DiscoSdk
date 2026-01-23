namespace DiscoSdk.Models.Messages.Mentions;

/// <summary>
/// Concrete mention builder that exposes the semantic mention API
/// without being tied to a specific message action.
///
/// This type can be used standalone to compose and analyze mentions,
/// or embedded inside higher-level builders (such as message or embed
/// builders) that need mention support without re-implementing the
/// mention-tracking logic.
///
/// It inherits all behavior from <see cref="MentionBuilderBase{TSelf}"/>,
/// providing a ready-to-use implementation for scenarios where no
/// additional state or behavior is required.
/// </summary>
public class MentionBuilder : MentionBuilderBase<MentionBuilder>
{
}
