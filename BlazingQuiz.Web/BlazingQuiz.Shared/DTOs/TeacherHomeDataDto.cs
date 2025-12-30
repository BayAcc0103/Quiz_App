using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public record TeacherHomeDataDto(int TotalQuizzes, int TotalQuestions, int TotalCategories, List<NotificationDto> FeedbackNotifications);
}