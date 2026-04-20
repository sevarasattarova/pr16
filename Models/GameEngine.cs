using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace pr16.Models
{
    public class GameEngine
    {
        public Player Player { get; private set; }
        public List<Enemy> CurrentEnemies { get; private set; }
        public Random Rng { get; private set; }
        public bool IsPlayerTurn { get; set; } = true;
        public bool IsBattleOver { get; private set; } = false;
        public bool IsGameOver { get; private set; } = false;
        public bool IsWaitingForChoice { get; set; } = false; // выбор предмета

        public GameEngine()
        {
            Player = new Player(100);
            Rng = new Random();
            CurrentEnemies = new List<Enemy>();
        }

        public void StartBattle(Enemy enemy)
        {
            CurrentEnemies.Clear();
            CurrentEnemies.Add(enemy);
            // Иногда добавляем до 3 врагов (не боссов)
            if (!enemy.IsBoss && Rng.Next(1, 4) > 1)
            {
                int extra = Rng.Next(1, 3);
                for (int i = 0; i < extra; i++)
                    CurrentEnemies.Add(CreateRandomEnemy(false));
            }
            IsPlayerTurn = true;
            IsBattleOver = false;
        }

        public Enemy CreateRandomEnemy(bool isBoss)
        {
            int type = Rng.Next(0, 3);
            if (isBoss)
            {
                // особые боссы
                int bossType = Rng.Next(0, 4);
                switch (bossType)
                {
                    case 0: return new Goblin(true);
                    case 1: return new Skeleton(true);
                    case 2: return new Mage(true);
                    default: return new Skeleton(true) { Name = "Пестов С—", Health = 52, Attack = 18, Defense = 3 }; // особый
                }
            }
            else
            {
                switch (type)
                {
                    case 0: return new Goblin();
                    case 1: return new Skeleton();
                    default: return new Mage();
                }
            }
        }

        public void PlayerAttack()
        {
            if (CurrentEnemies.Count == 0) return;
            var target = CurrentEnemies[0];
            int damage = Player.CurrentWeapon.Damage;
            if (Rng.Next(100) < Player.CurrentWeapon.CritChance)
                damage *= 2;
            target.TakeDamage(damage);
            Player.EventLog.Add($"Вы атаковали {target.Name} и нанесли {damage} урона.");
            if (target.Health <= 0)
            {
                Player.EventLog.Add($"{target.Name} повержен!");
                CurrentEnemies.Remove(target);
                if (CurrentEnemies.Count == 0)
                {
                    IsBattleOver = true;
                    Player.EventLog.Add("Бой окончен!");
                    return;
                }
            }
            IsPlayerTurn = false;
        }

        public void PlayerDefend()
        {
            // защита дает 40% шанс увернуться, иначе блок 70-100% от защиты
            bool dodged = Rng.Next(100) < 40;
            if (dodged)
            {
                Player.EventLog.Add("Вы увернулись от атаки!");
                IsPlayerTurn = false;
                return;
            }
            else
            {
                // блок: уменьшение урона на 70-100% от защиты
                // но урон будет применён при атаке врага
                // сохраняем флаг, что в этом ходу активна защита
                Player.EventLog.Add("Вы приготовились защищаться.");
                // реализуем в EnemyAttack: если защита активна, урон снижается
                IsPlayerTurn = false;
            }
        }

        public void EnemyAttack(bool isDefending)
        {
            foreach (var enemy in CurrentEnemies.ToList())
            {
                if (Player.Health <= 0) break;
                if (enemy.Health <= 0) continue;

                int baseDamage = enemy.CalculateDamage(Player, Rng);
                bool ignoreArmor = (enemy is Skeleton);
                if (isDefending)
                {
                    int reductionPercent = Rng.Next(70, 101); // 70-100%
                    int reducedDamage = baseDamage * reductionPercent / 100;
                    if (ignoreArmor) reducedDamage = baseDamage; // скелет игнорит защиту
                    Player.TakeDamage(reducedDamage, ignoreArmor);
                    Player.EventLog.Add($"{enemy.Name} атаковал и нанёс {reducedDamage} урона (блок {reductionPercent}%).");
                }
                else
                {
                    Player.TakeDamage(baseDamage, ignoreArmor);
                    Player.EventLog.Add($"{enemy.Name} атаковал и нанёс {baseDamage} урона.");
                }
                enemy.ApplySpecialAbility(Player, Rng);
                if (Player.IsStunned)
                {
                    Player.EventLog.Add("Вы заморожены и пропускаете следующий ход!");
                    IsPlayerTurn = false;
                    Player.IsStunned = false;
                    return;
                }
            }
            if (Player.Health <= 0)
            {
                IsGameOver = true;
                Player.EventLog.Add("Игрок погиб... Игра окончена.");
            }
            IsPlayerTurn = true;
        }

        public Item GenerateRandomItem()
        {
            int type = Rng.Next(0, 3);
            switch (type)
            {
                case 0:
                    return new HealthPotion();
                case 1:
                    string[] wepNames = { "Меч", "Топор", "Кинжал", "Булава" };
                    return new Weapon(wepNames[Rng.Next(wepNames.Length)], Rng.Next(8, 20), Rng.Next(0, 30));
                default:
                    string[] armNames = { "Кольчуга", "Латный доспех", "Кожаная броня" };
                    return new Armor(armNames[Rng.Next(armNames.Length)], Rng.Next(3, 12));
            }
        }

        public void ApplyItem(Item item)
        {
            if (item is HealthPotion)
            {
                Player.Heal(Player.MaxHealth);
                Player.EventLog.Add("Вы выпили зелье и полностью восстановили здоровье!");
            }
            else if (item is Weapon newWeapon)
            {
                // нужно показать выбор пользователю - делаем через событие
                IsWaitingForChoice = true;
                // сохраняем предмет для выбора
                PendingItem = newWeapon;
            }
            else if (item is Armor newArmor)
            {
                IsWaitingForChoice = true;
                PendingItem = newArmor;
            }
        }

        public object PendingItem { get; set; }

        public void TakeNewItem()
        {
            if (PendingItem is Weapon w)
            {
                Player.EquipWeapon(w);
                Player.EventLog.Add($"Вы взяли {w.Name} (урон {w.Damage}, крит {w.CritChance}%).");
            }
            else if (PendingItem is Armor a)
            {
                Player.EquipArmor(a);
                Player.EventLog.Add($"Вы надели {a.Name} (защита {a.Defense}).");
            }
            PendingItem = null;
            IsWaitingForChoice = false;
        }

        public void DiscardItem()
        {
            Player.EventLog.Add($"Вы выбросили {((Item)PendingItem).Name}.");
            PendingItem = null;
            IsWaitingForChoice = false;
        }
    }
}