using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimWinInput;

namespace POEStashSorter
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private int delay = 40;
		private FetchJsonManager jsonManager = null;
		private List<string> notSortedStash = new List<string>();
		private List<string> sortedStash = new List<string>();
		public MainWindow()
		{
			InitializeComponent();
			string appdataPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
			ConfigManager.FileName = $"{appdataPath}\\POEStashSorter\\config.xml";
			ConfigManager.Load();
			AccountNameTextBox.Text = ConfigManager.Parameters.AccountName;
			LeagueTextBox.Text = ConfigManager.Parameters.League;
			CookieTextBox.Text = ConfigManager.Parameters.Cookie;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			string cookie = CookieTextBox.Text;
			string accountName = AccountNameTextBox.Text;
			string league = LeagueTextBox.Text;
			string tab = StashTabTextBox.Text;
			new Thread(() => Sort(cookie, accountName, league, tab)).Start();
			/*int i = 0;
			while (i++ < 10)
				Swap(0, i, 0, i+1);*/
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string cookie = CookieTextBox.Text;
			string accountName = AccountNameTextBox.Text;
			string league = LeagueTextBox.Text;
			string tab = StashTabTextBox.Text;
			new Thread(() => Get3xMaps(cookie, accountName, league, tab)).Start();

		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			string cookie = CookieTextBox.Text;
			string accountName = AccountNameTextBox.Text;
			string league = LeagueTextBox.Text;
			string tab = StashTabTextBox.Text;
			new Thread(() => GetEachMapsByOne(cookie, accountName, league, tab)).Start();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void GetEachMapsByOne(string cookie, string accountName, string league, string tab)
		{
			if (jsonManager == null)
				jsonManager = new FetchJsonManager(cookie, accountName, league);
			JsonResponse json = jsonManager.FetchStashTabJSON(tab);
			sortedStash.Clear();
			//sortedStash.AddRange(json.items.OrderBy(x => x.properties?[0]?.name).ThenBy(x => x.typeLine).Select(x => x.typeLine).ToArray());
			var mapsGroup = json.items.
				Where(x => x.category.ToString() == "maps").
				//Select(x => x.ItemBaseType).
				//ToArray().
				GroupBy(x => x.ItemBaseType);
			foreach (var maps in mapsGroup)
			{
				var item = maps.ElementAt(0);
				MoveToInventory(item.x, item.y);
			}
		}

		private void Get3xMaps(string cookie, string accountName, string league, string tab)
		{
			if (jsonManager == null)
				jsonManager = new FetchJsonManager(cookie, accountName, league);
			JsonResponse json = jsonManager.FetchStashTabJSON(tab);
			sortedStash.Clear();
			//sortedStash.AddRange(json.items.OrderBy(x => x.properties?[0]?.name).ThenBy(x => x.typeLine).Select(x => x.typeLine).ToArray());
			var mapsGroup = json.items.
				Where(x => x.category.ToString() == "maps").
				GroupBy(x => x.ItemBaseType);
			foreach (var maps in mapsGroup)
			{
				int count3x = maps.Count() / 3;
				for (int pos = 0; pos < count3x * 3; pos++)
				{
					var item = maps.ElementAt(pos);
					MoveToInventory(item.x, item.y);
				}
			}
		}

		private void Sort(string cookie, string accountName, string league, string tab)
		{
			if (jsonManager == null)
				jsonManager = new FetchJsonManager(cookie, accountName, league);
			JsonResponse json = jsonManager.FetchStashTabJSON(tab);
			notSortedStash.Clear();
			//notSortedStash.AddRange(json.items.ConvertAll(x => x?.typeLine).ToArray());
			notSortedStash.AddRange(new string[144]);
			foreach (var item in json.items)
				notSortedStash[item.x * 12 + item.y] = item.ItemBaseType;
			sortedStash.Clear();
			//sortedStash.AddRange(json.items.OrderBy(x => x.properties?[0]?.name).ThenBy(x => x.typeLine).Select(x => x.typeLine).ToArray());
			sortedStash.AddRange(json.items.
				OrderBy(x => x.category.ToString()).
				ThenBy(x => int.Parse(x.properties?.FirstOrDefault(y => y.name == "Map Tier")?.values?.FirstOrDefault()?.FirstOrDefault() ?? "-1")).
				ThenBy(x => x.ItemBaseType).
				Select(x => x.ItemBaseType).
				ToArray()
			);
			for (int x = 0; x <= 11; x++)
			{
				for (int y = 0; y <= 11; y++)
				{
					if (x * 12 + y >= sortedStash.Count) break;
					string itemInSortedStash = sortedStash[x * 12 + y];
					string itemInNotSortedStash = notSortedStash[x * 12 + y];

					int indexOfFirstItemInNotSortedStash = notSortedStash.IndexOf(itemInSortedStash, x * 12 + y);
					int xOfFirstItemInNotSortedStash = indexOfFirstItemInNotSortedStash / 12;
					int yOfFirstItemInNotSortedStash = indexOfFirstItemInNotSortedStash % 12;
					notSortedStash[x * 12 + y] = null;
					notSortedStash[xOfFirstItemInNotSortedStash * 12 + yOfFirstItemInNotSortedStash] = itemInNotSortedStash;
					if (itemInSortedStash != itemInNotSortedStash)
						if (itemInNotSortedStash != null)
							Swap(xOfFirstItemInNotSortedStash, yOfFirstItemInNotSortedStash, x, y);
						else
							Move(xOfFirstItemInNotSortedStash, yOfFirstItemInNotSortedStash, x, y);
				}
			}

			foreach (var i in sortedStash)
			{

			}
		}

		private void MoveToInventory(int fromX, int fromY)
		{
			int cellSize = 48;
			fromX = fromX * cellSize + 40;
			fromY = fromY * cellSize + 195;
			ClickWithMod(fromX, fromY, 0xA2);
		}

		private void Move(int fromX, int fromY, int toX, int toY)
		{
			int cellSize = 48;
			fromX = fromX * cellSize + 40;
			fromY = fromY * cellSize + 195;
			toX = toX * cellSize + 40;
			toY = toY * cellSize + 195;
			Click(fromX, fromY);
			Click(toX, toY);
		}

		private void Swap(int fromX, int fromY, int toX, int toY)
		{
			int cellSize = 48;
			fromX = fromX * cellSize + 40;
			fromY = fromY * cellSize + 195;
			toX = toX * cellSize + 40;
			toY = toY * cellSize + 195;
			Click(fromX, fromY);
			Click(toX, toY);
			Click(fromX, fromY);
		}

		private void Click(int x, int y)
		{
			Thread.Sleep(delay);
			SimMouse.Act(SimMouse.Action.MoveOnly, x, y);
			Thread.Sleep(delay);
			SimMouse.Act(SimMouse.Action.LeftButtonDown, x, y);
			Thread.Sleep(delay);
			SimMouse.Act(SimMouse.Action.LeftButtonUp, x, y);
			Thread.Sleep(delay);

		}

		private void ClickWithMod(int x, int y, byte key)
		{
			Thread.Sleep(delay);
			SimKeyboard.KeyDown(key);
			Thread.Sleep(delay);
			SimMouse.Act(SimMouse.Action.MoveOnly, x, y);
			Thread.Sleep(delay);
			SimMouse.Act(SimMouse.Action.LeftButtonDown, x, y);
			Thread.Sleep(delay);
			SimMouse.Act(SimMouse.Action.LeftButtonUp, x, y);
			Thread.Sleep(delay);
			SimKeyboard.KeyUp(key);
			Thread.Sleep(delay);

		}

		private void AccountNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ConfigManager.Parameters.AccountName = AccountNameTextBox.Text;
			ConfigManager.Save();
		}

		private void LeagueTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ConfigManager.Parameters.League = LeagueTextBox.Text;
			ConfigManager.Save();
		}

		private void CookieTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ConfigManager.Parameters.Cookie = CookieTextBox.Text;
			ConfigManager.Save();
		}
	}

}
