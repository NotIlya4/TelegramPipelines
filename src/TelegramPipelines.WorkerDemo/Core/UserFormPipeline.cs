using Telegram.Bot;
using TelegramPipelines.TelegramPipeline;

namespace TelegramPipelines.Demo.Core;

public class UserFormPipeline : ITelegramPipelineClass<UserForm>
{
    public async Task<UserFormPipelineContext> GetStatus(TelegramPipelineContext context)
    {
        return await context.LocalStorage.Get<UserFormPipelineContext>("status") ??
               new UserFormPipelineContext(UserFormPipelineStatus.FirstNameAsk, new UserForm("", "", ""));
    }

    public async Task SaveStatus(TelegramPipelineContext context, UserFormPipelineContext status)
    {
        await context.LocalStorage.Save("status", status);
    }

    public async Task<UserForm?> Execute(TelegramPipelineContext context)
    {
        UserFormPipelineContext status = await GetStatus(context);
        if (status.Status == UserFormPipelineStatus.FirstNameAsk)
        {
            await AskFirstName(context);
        }
        
        status = await GetStatus(context);
        if (status.Status == UserFormPipelineStatus.LastNameAsk)
        {
            await AskLastName(context);
        }
        
        status = await GetStatus(context);
        UserForm? userForm = null;
        if (status.Status == UserFormPipelineStatus.AgeAsk)
        {
            userForm = await AskAge(context);
        }

        return userForm;
    }

    public async Task AskFirstName(TelegramPipelineContext context)
    {
        UserFormPipelineContext status = await GetStatus(context);
        
        string? firstname = await context.NestedPipelineExecutor.Execute("firstname", new AskUserPipeline("Type your name"));
        if (firstname is not null)
        {
            status = new UserFormPipelineContext(Status: UserFormPipelineStatus.LastNameAsk,
                Form: status.Form with { FirstName = firstname });
            await SaveStatus(context, status);
        }
    }
    
    public async Task AskLastName(TelegramPipelineContext context)
    {
        UserFormPipelineContext status = await GetStatus(context);
        
        string? lastname = await context.NestedPipelineExecutor.Execute("lastname", new AskUserPipeline("Type your lastname"));
        if (lastname is not null)
        {
            status = new UserFormPipelineContext(UserFormPipelineStatus.AgeAsk,
                Form: status.Form with { LastName = lastname });
            await SaveStatus(context, status);
        }
    }
    
    public async Task<UserForm?> AskAge(TelegramPipelineContext context)
    {
        UserFormPipelineContext status = await GetStatus(context);
        
        string? age = await context.NestedPipelineExecutor.Execute("age", new AskUserPipeline("Type your age"));
        if (age is not null)
        {
            status = status with { Form = status.Form with { Age = age } };
            return status.Form;
        }

        return null;
    }
}

public record UserFormPipelineContext(UserFormPipelineStatus Status, UserForm Form);

public enum UserFormPipelineStatus
{
    FirstNameAsk,
    LastNameAsk,
    AgeAsk
}

public record UserForm(string FirstName, string LastName, string Age);