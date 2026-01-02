using NotificationEntity = Grc.Domain.Notification.Notification;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Notification;
using Grc.Domain.Notification;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Notification;

[Authorize]
public class NotificationAppService : BasePolicyAppService, INotificationAppService
{
    private readonly INotificationRepository _repository;

    public NotificationAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        INotificationRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<PagedResultDto<NotificationDto>> GetListAsync(NotificationListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<NotificationDto>(
            totalCount,
            ObjectMapper.Map<List<NotificationEntity>, List<NotificationDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<NotificationEntity>> CreateFilteredQueryAsync(NotificationListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(n => n.Title.Contains(input.Filter) || n.Message.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Type))
        {
            query = query.Where(n => n.NotificationType == input.Type);
        }

        if (input.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == input.IsRead.Value);
        }

        if (input.CreatedFrom.HasValue)
        {
            query = query.Where(n => n.CreationTime >= input.CreatedFrom.Value);
        }

        if (input.CreatedTo.HasValue)
        {
            query = query.Where(n => n.CreationTime <= input.CreatedTo.Value);
        }

        return query;
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.CountAsync(query.Where(n => !n.IsRead));
    }

    public async Task MarkAllAsReadAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var unreadNotifications = await AsyncExecuter.ToListAsync(query.Where(n => !n.IsRead));

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
            notification.Status = "Read";
            await _repository.UpdateAsync(notification);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [Authorize(GrcPermissions.Notifications.Manage)]
    public async Task<NotificationDto> CreateAsync(CreateNotificationDto input)
    {
        var entity = new NotificationEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            NotificationType = input.NotificationType,
            Title = input.Title,
            Message = input.Message,
            RecipientId = input.RecipientId,
            RecipientEmail = input.RecipientEmail,
            Priority = input.Priority,
            ActionUrl = input.ActionUrl,
            SentDate = DateTime.UtcNow,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "Notification", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<NotificationEntity, NotificationDto>(entity);
    }

    [Authorize(GrcPermissions.Notifications.Manage)]
    public async Task<NotificationDto> UpdateAsync(Guid id, UpdateNotificationDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.NotificationType = input.NotificationType;
        entity.Title = input.Title;
        entity.Message = input.Message;
        entity.Priority = input.Priority;
        entity.ActionUrl = input.ActionUrl;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "Notification", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<NotificationEntity, NotificationDto>(entity);
    }

    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<NotificationDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<NotificationEntity, NotificationDto>(entity);
    }

    [Authorize(GrcPermissions.Notifications.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    [Authorize(GrcPermissions.Notifications.View)]
    public async Task MarkAsReadAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.IsRead = true;
        entity.ReadDate = DateTime.UtcNow;
        entity.Status = "Read";

        await _repository.UpdateAsync(entity, autoSave: true);
    }
}
