using System;
using System.Collections.Generic;
using Duties;

namespace Do
{
    public class Config
    {
        private static Lazy<Config> _active = new Lazy<Config>(() => new Config());
        public static Config Active => _active.Value;
        
        public Config()
        {
            
        }

        public IEnumerable<Duty.T> ActiveDuties()
        {
            yield return Duty.create(@"E:\Dropbox\the library\the library\todo\duty");
        }
    }
}