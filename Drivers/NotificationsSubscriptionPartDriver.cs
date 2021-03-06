﻿using DQ.Scheduling.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin;

namespace DQ.Scheduling.Drivers
{
    [OrchardFeature("DQ.SchedulingNotifications")]
    public class NotificationsSubscriptionPartDriver : ContentPartDriver<NotificationsSubscriptionPart> {

        private readonly IWorkContextAccessor _workContextAccessor;

        public NotificationsSubscriptionPartDriver(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(NotificationsSubscriptionPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_NotificationsSubscription", 
                    () => shapeHelper.Parts_NotificationsSubscription()),
                ContentShape("Parts_NotificationsSubscription_UnSubscribe",
                    () => shapeHelper.Parts_NotificationsSubscription_UnSubscribe())
            );
        }

        protected override DriverResult Editor(NotificationsSubscriptionPart part, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_NotificationsSubscription_Edit", () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/NotificationsSubscription",
                    Prefix: Prefix,
                    Model: part
                )),
                ContentShape("Parts_NotificationSubscription_Edit_Admin", () => {

                    // Only from dashboard
                    if (!AdminFilter.IsApplied(_workContextAccessor.GetContext().HttpContext.Request.RequestContext))
                        return null;

                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/NotificationsSubscription.Admin",
                        Prefix: Prefix,
                        Model: part
                    );
                })
            );
        }

        protected override DriverResult Editor(NotificationsSubscriptionPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);

            // TODO: use chosen subscribe type
            part.SubscribeType = SubscribeType.Email;

            if (part.SubscribeType == SubscribeType.Email || part.SubscribeType == SubscribeType.Both && string.IsNullOrEmpty(part.Email)) {

                var currentUser = _workContextAccessor.GetContext().CurrentUser;

                if (currentUser == null) {
                    // Email should be filled in
                    if (string.IsNullOrEmpty(part.Email)) {
                        updater.AddModelError("Email", T("Email is mandatory"));
                    }
                }
                else {
                    // Admin can change email, so do not always base on current user
                    if (string.IsNullOrEmpty(part.Email)) {
                        part.Email = currentUser.Email;
                        part.UserId = currentUser.Id;
                    }
                }
            }
            if (part.SubscribeType == SubscribeType.Sms || part.SubscribeType == SubscribeType.Both && string.IsNullOrEmpty(part.Phone)) {
                updater.AddModelError("Email", T("Phone number is mandatory"));
            }

            return Editor(part, shapeHelper);
        }
    }
}