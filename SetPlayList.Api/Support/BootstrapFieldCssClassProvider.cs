using Microsoft.AspNetCore.Components.Forms;

namespace SetPlayList.Api.Support;

public sealed class BootstrapFieldCssClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();

        return (isValid) switch
        {
            true => "",
            false => "is-invalid",
        };
    }
}
