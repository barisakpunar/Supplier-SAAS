using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.OpenAccount;

/// <summary>
/// Represents settings of the open account payment plugin
/// </summary>
public class OpenAccountPaymentSettings : ISettings
{
    /// <summary>
    /// Gets or sets description text shown on checkout payment info step
    /// </summary>
    public string DescriptionText { get; set; }
}
