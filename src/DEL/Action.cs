﻿using System;
using System.Collections.Generic;

namespace ImplicitCoodrination.DEL
{
    public class Action
    {
        public HashSet<Event> events;
        public HashSet<Event> designatedEvents;
        public Dictionary<Agent, Tuple<Event, Event>> accessibility;


        public Action()
        {
            
        }
    }
}