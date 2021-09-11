using System;
using System.Collections.Generic;
using System.Linq;
using Do.Core;
using Duties;

namespace Do
{
    public class Config
    {
        private static Lazy<Config> _active = new Lazy<Config>(() => new Config());
        public static Config Active => _active.Value;

        private List<Duty.T> _duties;
        
        public Config()
        {
            _duties = ActiveDuties().ToList();
        }

        public IEnumerable<Duty.T> ActiveDuties()
        {
            foreach (var duty in ApplicationConfiguration.load().Duties)
            {
                yield return Duty.create(duty);
            }
        }

        public Duty.T DefaultDuty() => _duties.First();
    }
}