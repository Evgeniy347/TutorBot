using Microsoft.AspNetCore.Mvc; 
using System.Text.Json;
using System.Text.Json.Serialization; 
using TutorBot.TelegramService.BotActions;

namespace TutorBot.TelegramService.FakeService
{
	[Route("api/v2/schedule")]
	[ApiController]
	public class FakeScheduleController : ControllerBase
	{
		public static JsonSerializerOptions DefaultOptions => new JsonSerializerOptions
		{
			PropertyNamingPolicy = null,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		ScheduleAction.GroupInfo[] _groups = [
            new ScheduleAction.GroupInfo(62140, 62387, 1, "ÐÈÌ-151001"),
            new ScheduleAction.GroupInfo(62141, 62387, 1, "ÐÈÌ-151001 fake"),
            new ScheduleAction.GroupInfo(62222, 62387, 1, "ÐÈÌ-151002"),
        ];
		 
		[HttpGet("groups")]
		public IActionResult GetToken([FromQuery] string search)
		{
			ScheduleAction.GroupInfo[] response = _groups.Where(x => x.Title?.Contains(search) ?? true).ToArray(); 
			return new JsonResult(response);
		}
	}
}
