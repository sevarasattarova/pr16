using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pr16.Models
{
    public abstract class Enemy
    {
        public string Name { get;  set; }
        public int Health { get; set; }
        public int Attack { get;  set; }
        public int Defense { get; set; }
        public bool IsBoss { get; set; } = false;

        public abstract void ApplySpecialAbility(Player player, Random rng);
        public abstract int CalculateDamage(Player player, Random rng);

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }
    }

    public class Goblin : Enemy
    {
        public Goblin(bool isBoss = false)
        {
            Name = isBoss ? "Вождь гоблинов" : "Гоблин";
            Health = isBoss ? 60 : 30;
            Attack = isBoss ? 18 : 12;
            Defense = isBoss ? 4 : 3;
            IsBoss = isBoss;
        }

        public override int CalculateDamage(Player player, Random rng)
        {
            int dmg = Attack;
            if (rng.Next(100) < 20) // 20% крит
                dmg *= 2;
            return dmg;
        }

        public override void ApplySpecialAbility(Player player, Random rng) { }
    }

    public class Skeleton : Enemy
    {
        public Skeleton(bool isBoss = false)
        {
            Name = isBoss ? "Ковальский" : "Скелет";
            Health = isBoss ? 100 : 40;
            Attack = isBoss ? 13 : 10;
            Defense = isBoss ? 7 : 5;
            IsBoss = isBoss;
        }

        public override int CalculateDamage(Player player, Random rng)
        {
            return Attack;
        }

        public override void ApplySpecialAbility(Player player, Random rng) { }
    }

    public class Mage : Enemy
    {
        public Mage(bool isBoss = false)
        {
            Name = isBoss ? "Архимаг C++" : "Маг";
            Health = isBoss ? 45 : 25;
            Attack = isBoss ? 24 : 15;
            Defense = isBoss ? 3 : 2;
            IsBoss = isBoss;
        }

        public override int CalculateDamage(Player player, Random rng)
        {
            return Attack;
        }

        public override void ApplySpecialAbility(Player player, Random rng)
        {
            if (rng.Next(100) < (IsBoss ? 25 : 15))
                player.IsStunned = true;
        }
    }
}