using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace biopot.Models
{
    public class PatientsInformation : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
            }
        }

        private string _sex;
        public string Sex
        {
            get => _sex;
            set => SetProperty(ref _sex, value);
        }

        private string _position;
        public string Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        private string _team;
        public string Team
        {
            get => _team;
            set => SetProperty(ref _team, value);
        }

        private string _lastMatch;
        public string LastMatch
        {
            get => _lastMatch;
            set => SetProperty(ref _lastMatch, value);
        }

        private bool _concussed72Hr;
        public bool Concussed72Hr
        {
            get => _concussed72Hr;
            set => SetProperty(ref _concussed72Hr, value);
        }

        private string _lastConcussion;
        public string LastConcussion
        {
            get => _lastConcussion;
            set => SetProperty(ref _lastConcussion, value);
        }

        private string _lastHIA;
        public string LastHIA
        {
            get => _lastHIA;
            set => SetProperty(ref _lastHIA, value);
        }

        private string _symptoms;
        public string Symptoms
        {
            get => _symptoms;
            set => SetProperty(ref _symptoms, value);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
    }
}
