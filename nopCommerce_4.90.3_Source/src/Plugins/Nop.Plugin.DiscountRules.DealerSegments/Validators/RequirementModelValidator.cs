using FluentValidation;
using Nop.Plugin.DiscountRules.DealerSegments.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.DealerSegments.Validators;

/// <summary>
/// Represents an <see cref="RequirementModel"/> validator.
/// </summary>
public class RequirementModelValidator : BaseNopValidator<RequirementModel>
{
    public RequirementModelValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.DiscountId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.DealerSegments.Fields.DiscountId.Required"));
        RuleFor(model => model.DealerSegmentId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.DealerSegments.Fields.DealerSegmentId.Required"));
    }
}