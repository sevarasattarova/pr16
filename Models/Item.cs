using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pr16.Models
{
    public abstract class Item
    {
        public string Name { get; protected set; }
    }

    public class Weapon : Item
    {
        public int Damage { get; private set; }
        public int CritChance { get; private set; } // 0-100

        public Weapon(string name, int damage, int critChance)
        {
            Name = name;
            Damage = damage;
            CritChance = critChance;
        }
    }

    public class Armor : Item
    {
        public int Defense { get; private set; }

        public Armor(string name, int defense)
        {
            Name = name;
            Defense = defense;
        }
    }

    public class HealthPotion : Item
    {
        public HealthPotion()
        {
            Name = "Целебное зелье";
        }
    }
}