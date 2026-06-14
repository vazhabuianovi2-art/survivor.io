using System;

namespace SurvivorIO
{
    /// <summary>
    /// A single upgrade the player can pick on level up. Plain data + an action
    /// that mutates the relevant player component when chosen.
    /// </summary>
    public class Skill
    {
        public string Name { get; }
        public string Description { get; }
        public Action Apply { get; }

        public Skill(string name, string description, Action apply)
        {
            Name = name;
            Description = description;
            Apply = apply;
        }
    }
}
