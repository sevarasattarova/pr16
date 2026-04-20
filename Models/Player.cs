using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace pr16.Models
{
    public class Player
    {
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public Weapon CurrentWeapon { get; private set; }
        public Armor CurrentArmor { get; private set; }
        public int Floor { get; set; } = 1;
        public List<string> EventLog { get; private set; } = new List<string>();
        public bool IsStunned { get; set; } = false;

        public Player(int maxHealth)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            CurrentWeapon = new Weapon("Кулак", 5, 0);
            CurrentArmor = new Armor("Лохмотья", 2);
        }

        public void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public void TakeDamage(int damage, bool ignoreArmor = false)
        {
            int finalDamage = damage;
            if (!ignoreArmor && CurrentArmor != null)
                finalDamage = Math.Max(1, damage - CurrentArmor.Defense);
            Health -= finalDamage;
            if (Health < 0) Health = 0;
        }

        public void EquipWeapon(Weapon newWeapon)
        {
            CurrentWeapon = newWeapon;
        }

        public void EquipArmor(Armor newArmor)
        {
            CurrentArmor = newArmor;
        }
    }
}