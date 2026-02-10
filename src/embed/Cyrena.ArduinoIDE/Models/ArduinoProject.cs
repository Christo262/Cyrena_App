using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.ArduinoIDE.Models
{
    public class ArduinoProject : Project
    {
        internal const string TypeId = "arduino-ide";
        internal const string BoardProp = "board";
        internal const string RamProp = "ram";
        internal const string ClockProp = "clock";

        public ArduinoProject()
        {
            Type = TypeId;
        }

        public ArduinoProject(Project project)
        {
            Name = project.Name;
            Description = project.Description;
            RootDirectory = project.RootDirectory;
            Created = project.Created;
            LastModified = project.LastModified;
            Type = TypeId;
            ConnectionId = project.ConnectionId;
            Properties = project.Properties;
        }

        [Required]
        public string? Board
        {
            get
            {
                return Properties[BoardProp];
            }
            set
            {
                Properties[BoardProp] = value;
            }
        }

        [Required]
        public string? Ram
        {
            get
            {
                return Properties[RamProp];
            }
            set
            {
                Properties[RamProp] = value;
            }
        }

        [Required]
        public string? Clock
        {
            get
            {
                return Properties[ClockProp];
            }
            set
            {
                Properties[ClockProp] = value;
            }
        }
    }
}
