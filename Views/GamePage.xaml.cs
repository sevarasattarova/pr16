using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using pr16.Models;

namespace pr16.Views
{
    public partial class GamePage : Page
    {
        private GameEngine _engine;
        private bool _isDefendingThisTurn = false;
        private int _stepsSinceLastBoss = 0;

        public GamePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _engine = new GameEngine();
            UpdateUI();
            ExploreButton.Visibility = Visibility.Visible;
            AttackButton.Visibility = Visibility.Collapsed;
            DefendButton.Visibility = Visibility.Collapsed;
            EventDescription.Text = "Нажмите 'Исследовать' чтобы начать приключение";
            EventImage.Source = new BitmapImage(new Uri("/Images/dungeon.png", UriKind.Relative));
        }

        private void UpdateUI()
            {
                FloorText.Text = _engine.Player.Floor.ToString();
                HealthText.Text = $"{_engine.Player.Health}/{_engine.Player.MaxHealth}";
                WeaponText.Text = $"{_engine.Player.CurrentWeapon.Name} ({_engine.Player.CurrentWeapon.Damage} урона)";
                ArmorText.Text = $"{_engine.Player.CurrentArmor.Name} (защ. {_engine.Player.CurrentArmor.Defense})";

                EventLogList.ItemsSource = null;
                EventLogList.ItemsSource = _engine.Player.EventLog;

                // ПРОВЕРКА: прокручиваем только если есть элементы
                if (EventLogList.Items.Count > 0)
                {
                    EventLogList.ScrollIntoView(EventLogList.Items[EventLogList.Items.Count - 1]);
                }
        }

        private  void Explore_Click(object sender, RoutedEventArgs e)
        {
            ExploreButton.IsEnabled = false;
            // определение события: враг или сундук (50/50), каждые 10 ходов босс
            _stepsSinceLastBoss++;
            bool isBossFight = (_stepsSinceLastBoss % 10 == 0);
            bool isChest = (!isBossFight && new Random().Next(2) == 0);

            if (isBossFight)
            {
                var boss = _engine.CreateRandomEnemy(true);
                _engine.StartBattle(boss);
                _engine.Player.EventLog.Add($"Вас поджидает БОСС: {boss.Name}!");
                StartBattleUI();
            }
            else if (isChest)
            {
                var item = _engine.GenerateRandomItem();
                _engine.Player.EventLog.Add($"Вы нашли сундук! Внутри: {item.Name}");
                EventImage.Source = new BitmapImage(new Uri("/Images/chest.png", UriKind.Relative));
                EventDescription.Text = $"Вы нашли: {item.Name}!";
                _engine.ApplyItem(item);
                if (_engine.IsWaitingForChoice)
                {
                    ShowItemChoiceUI(item);
                }
                else
                {
                    // зелье сразу применено
                    UpdateUI();
                    ExploreButton.IsEnabled = true;
                }
            }
            else
            {
                var enemy = _engine.CreateRandomEnemy(false);
                _engine.StartBattle(enemy);
                _engine.Player.EventLog.Add($"На вас напал {enemy.Name}!");
                StartBattleUI();
            }
            UpdateUI();
        }

        private void StartBattleUI()
        {
            ExploreButton.Visibility = Visibility.Collapsed;
            AttackButton.Visibility = Visibility.Visible;
            DefendButton.Visibility = Visibility.Visible;
            EventDescription.Text = $"Бой с {_engine.CurrentEnemies[0].Name}! Ваш ход.";
            EventImage.Source = new BitmapImage(new Uri("/Images/battle.png", UriKind.Relative));
            _isDefendingThisTurn = false;
        }

        private void Attack_Click(object sender, RoutedEventArgs e)
        {
            if (_engine.IsBattleOver) return;
            _engine.PlayerAttack();
            UpdateUI();
            if (_engine.IsBattleOver)
            {
                EndBattle();
                return;
            }
            // ход врага
            _engine.EnemyAttack(_isDefendingThisTurn);
            UpdateUI();
            if (_engine.IsGameOver)
            {
                EndGame();
                return;
            }
            _isDefendingThisTurn = false;
            if (_engine.IsBattleOver)
                EndBattle();
        }

        private void Defend_Click(object sender, RoutedEventArgs e)
        {
            if (_engine.IsBattleOver) return;
            _isDefendingThisTurn = true;
            _engine.PlayerDefend();
            UpdateUI();
            _engine.EnemyAttack(true);
            UpdateUI();
            if (_engine.IsGameOver)
            {
                EndGame();
                return;
            }
            _isDefendingThisTurn = false;
            if (_engine.IsBattleOver)
                EndBattle();
        }

        private void EndBattle()
        {
            _engine.Player.Floor++;
            _stepsSinceLastBoss++;
            AttackButton.Visibility = Visibility.Collapsed;
            DefendButton.Visibility = Visibility.Collapsed;
            ExploreButton.Visibility = Visibility.Visible;
            ExploreButton.IsEnabled = true;
            EventDescription.Text = "Вы победили! Продолжайте исследование.";
            EventImage.Source = new BitmapImage(new Uri("/Images/chest.png", UriKind.Relative));
            UpdateUI();
        }

        private void ShowItemChoiceUI(Item item)
        {
            AttackButton.Visibility = Visibility.Collapsed;
            DefendButton.Visibility = Visibility.Collapsed;
            ExploreButton.Visibility = Visibility.Collapsed;
            TakeButton.Visibility = Visibility.Visible;
            DiscardButton.Visibility = Visibility.Visible;
            string details = "";
            if (item is Weapon w)
                details = $"\nУрон: {w.Damage} | Крит: {w.CritChance}%\nТекущее оружие: {_engine.Player.CurrentWeapon.Name} (ур.{_engine.Player.CurrentWeapon.Damage})";
            else if (item is Armor a)
                details = $"\nЗащита: {a.Defense}\nТекущая броня: {_engine.Player.CurrentArmor.Name} (защ.{_engine.Player.CurrentArmor.Defense})";
            EventDescription.Text = $"Вы нашли {item.Name}!{details}\nВзять или выбросить?";
        }

        private void TakeItem_Click(object sender, RoutedEventArgs e)
        {
            _engine.TakeNewItem();
            TakeButton.Visibility = Visibility.Collapsed;
            DiscardButton.Visibility = Visibility.Collapsed;
            ExploreButton.Visibility = Visibility.Visible;
            ExploreButton.IsEnabled = true;
            EventDescription.Text = "Вы взяли предмет и продолжили путь.";
            UpdateUI();
        }

        private void DiscardItem_Click(object sender, RoutedEventArgs e)
        {
            _engine.DiscardItem();
            TakeButton.Visibility = Visibility.Collapsed;
            DiscardButton.Visibility = Visibility.Collapsed;
            ExploreButton.Visibility = Visibility.Visible;
            ExploreButton.IsEnabled = true;
            EventDescription.Text = "Вы выбросили предмет.";
            UpdateUI();
        }

        private void EndGame()
        {
            NavigationService.Navigate(new GameOverPage());
        }
    }
}