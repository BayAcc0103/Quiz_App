using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public record AdminHomeDataDto(int TotalCategories, int TotalStudents, int TotalQuizes, int ApprovedStudents, int ActiveQuizes, List<NotificationDto> CategoryNotifications, List<NotificationDto> FeedbackNotifications);
}
