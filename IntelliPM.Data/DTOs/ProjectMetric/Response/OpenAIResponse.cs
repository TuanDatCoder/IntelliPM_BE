using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Response
{
    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
        public class Choice
        {
            public Message Message { get; set; }
        }
        public class Message
        {
            public string Content { get; set; }
        }
    }
}
