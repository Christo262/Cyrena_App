using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Developer.Models
{
    public class SolutionViewModel
    {
        public SolutionViewModel(string rootDirectory)
        {
            Projects = new List<ProjectViewModel>();
            RootDirectory = rootDirectory;
        }
        public string RootDirectory { get; }
        public List<ProjectViewModel> Projects { get; set; }
    }
}
