﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;
using travel_agent.Controls;
using travel_agent.Models;
using travel_agent.Services;

namespace travel_agent.WindowsAndPages
{
	/// <summary>
	/// Interaction logic for AddAndModifyArangementPage.xaml
	/// </summary>
	public partial class AddAndModifyArangementPage : Page
	{
		private new readonly MainWindow Parent;
		private Application App;
		private ArrangementService ArrangementService;
		private PlaceService PlaceService;
		private BitmapImage Image = null;
		private Arrangement Arrangement;

		private Point startPoint;
		private int startIndex = -1;

		private ObservableCollection<Place> lastRearrengement = new ObservableCollection<Place>();
		private ListViewItem selectedListViewItem;
		private ArrangementStep selectedItem;

		public AddAndModifyArangementPage(MainWindow parent, Arrangement arrangement = null)
		{
			InitializeComponent();
			App = Application.Current;
			Parent = parent;
			ArrangementService = ArrangementService.Instance;
			PlaceService = PlaceService.Instance;
			Arrangement = arrangement;

			StartDatePicker.DateChanged += StartDatePicker_DateChanged;
			EndDatePicker.DateChanged += EndDatePicker_DateChanged;
			TransportListView.SelectionChanged += ListView_ItemClick;

			SetUpPage();

		}

		private List<Place> FilterPlaces(Place.PlaceType placeType)
		{
			List<Place> allPlaces = PlaceService.GetAllByType(placeType);
			if (Arrangement == null) return allPlaces;
			List<Place> places = (from place in allPlaces where !Arrangement.Places.Contains(place) select place).ToList();
			return places;
		}

		private List<Place> FilterByName(Place.PlaceType placeType, string name)
		{
			return FilterPlaces(placeType).Where(p => p.Name.ToLower().StartsWith(name.ToLower())).ToList();
		}

		private void SetUpPage()
		{
			ObservableCollection<Place> attractions = new ObservableCollection<Place>(FilterPlaces(Place.PlaceType.ATRACTION));
			ObservableCollection<Place> restaurants = new ObservableCollection<Place>(FilterPlaces(Place.PlaceType.RESTAURANT));
			ObservableCollection<Place> accommodation = new ObservableCollection<Place>(FilterPlaces(Place.PlaceType.ACCOMMODATION));

			if (attractions.Count == 0) {
				AttractionsList.Visibility = Visibility.Collapsed;
				NoContentAttraction.Visibility = Visibility.Visible;
			}
			else AttractionsList.ItemsSource = attractions;
			if (restaurants.Count == 0)
			{
				RestaurantsList.Visibility = Visibility.Collapsed;
				NoContentRestaurants.Visibility = Visibility.Visible;
			}
			else RestaurantsList.ItemsSource = restaurants;
			if (accommodation.Count == 0)
			{
				AccommodationList.Visibility = Visibility.Collapsed;
				NoContentAccommodation.Visibility = Visibility.Visible;
			}
			else AccommodationList.ItemsSource = accommodation;
			RearrangeListView.ItemsSource = new ObservableCollection<Place>();

			if (Arrangement == null) return;
			ArrangementNameInput.InputText = Arrangement.Name;
			SetImage(Arrangement.Image);
			StartDatePicker.SelectedDate = Arrangement.Start;
			EndDatePicker.SelectedDate = Arrangement.End;

		}

		private void SetImage(BitmapImage image)
		{
			Image = image;
			var imageBrush = new ImageBrush(Image);
			imageBrush.Stretch = Stretch.UniformToFill;
			imageBrush.AlignmentX = AlignmentX.Center;
			imageBrush.AlignmentY = AlignmentY.Center;
			PlaceImage.Background = imageBrush;
			PlaceImageLabel.Visibility = Visibility.Hidden;
			PlaceAddImageButton.Content = App.Resources["String.SwichImageButton"] as string;
		}

		private void OnAddPictureClick(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg;*.gif;*.bmp)|*.png;*.jpeg;*.jpg;*.gif;*.bmp";
			if (openFileDialog.ShowDialog() == true) SetImage(new BitmapImage(new Uri(openFileDialog.FileName)));
		}


		private void searchPlaceInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			TextBox innerTextBox = (TextBox)textBox.Template.FindName("PART_TextBox", textBox);
			string textSearch = innerTextBox.Text;
			switch ((tabControl.SelectedItem as TabItem).Header)
			{
				case "Atrakcije":
					List<Place> attractions = FilterByName(Place.PlaceType.ATRACTION, textSearch);
					if (attractions.Count == 0)
					{
						AttractionsList.Visibility = Visibility.Collapsed;
						NoContentAttraction.Visibility = Visibility.Visible;
					}
					else {
						AttractionsList.Visibility = Visibility.Visible;
						NoContentAttraction.Visibility = Visibility.Collapsed;
						AttractionsList.ItemsSource = attractions;
					}
					break;
				case "Restorani":
					List<Place> restaurants = FilterByName(Place.PlaceType.RESTAURANT, textSearch);
					if (restaurants.Count == 0)
					{
						RestaurantsList.Visibility = Visibility.Collapsed;
						NoContentRestaurants.Visibility = Visibility.Visible;
					}
					else { RestaurantsList.ItemsSource = restaurants;
						RestaurantsList.Visibility = Visibility.Visible;
						NoContentRestaurants.Visibility = Visibility.Collapsed;
					}
					break;
				case "Smeštaj":
					List<Place> accommodation = FilterByName(Place.PlaceType.ACCOMMODATION, textSearch);
					if (accommodation.Count == 0)
					{
						AccommodationList.Visibility = Visibility.Collapsed;
						NoContentAccommodation.Visibility = Visibility.Visible;
					}
					else { AccommodationList.ItemsSource = accommodation;
						AccommodationList.Visibility = Visibility.Visible;
						NoContentAccommodation.Visibility = Visibility.Collapsed;
					}
					break;
			}
		}


		private static T FindAnchestor<T>(DependencyObject current)
		where T : DependencyObject
		{
			do
			{
				if (current is T)
				{
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while (current != null);
			return null;
		}

		private void StartDatePicker_DateChanged(object sender, SelectionChangedEventArgs e)
		{
			DateTime selectedDate = (DateTime)((FancyDatePicker)sender).SelectedDate;
			if (EndDatePicker.SelectedDate == null || DateTime.Compare(selectedDate, (DateTime)EndDatePicker.SelectedDate) > 0)
			{
				selectedDate = selectedDate.AddDays(1);
				EndDatePicker.SelectedDate = selectedDate;
			}
		}

		private void EndDatePicker_DateChanged(object sender, SelectionChangedEventArgs args)
		{
			DateTime selectedDate = (DateTime)((FancyDatePicker)sender).SelectedDate;
			if (StartDatePicker.SelectedDate == null || DateTime.Compare(selectedDate, (DateTime)StartDatePicker.SelectedDate) < 0)
			{
				selectedDate = selectedDate.AddDays(-1);
				StartDatePicker.SelectedDate = selectedDate;
			}
		}

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			startPoint = e.GetPosition(null);
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{

			Point mousePosition = e.GetPosition(null);
			Vector diff = startPoint - mousePosition;

			if (e.LeftButton == MouseButtonState.Pressed)
			{

				if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					ListView listView = sender as ListView;
					ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
					if (listViewItem == null) return;

					Place item = (Place)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
					if (item == null) return;

					DataObject dragData = new DataObject();
					dragData.SetData("Place", item);
					dragData.SetData("SourceListView", listView);
					DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);

					if(RearrangeListView.Items.Count == 1)
					{
						NextButton.IsEnabled = false;
						NextButton.ToolTip = (string)Application.Current.FindResource("String.Add2Places");
					}

				}


			}
		}

		private void rearrangeListView_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("Place"))
			{
				Place place = e.Data.GetData("Place") as Place;
				ListView listView = sender as ListView;
				ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
				ObservableCollection<Place> places = listView.ItemsSource as ObservableCollection<Place>;

				if (listViewItem != null && listViewItem.DataContext is Place)
				{
					Place dropLocation = (Place)listViewItem.DataContext;
					int index = places.IndexOf(dropLocation);

					if (index >= 0)
					{
						places.Remove(place);
						places.Insert(index,place);
					}
				}
				else
				{
					places.Remove(place);
					places.Add(place);
				}

				if(RearrangeListView.Items.Count == 2)
				{
					NextButton.IsEnabled = true;
					NextButton.ToolTip = null;
				}


				switch (place.Type)
				{
					case Place.PlaceType.ATRACTION:
						(AttractionsList.ItemsSource as ObservableCollection<Place>).Remove(place);
						break;
					case Place.PlaceType.RESTAURANT:
						(RestaurantsList.ItemsSource as ObservableCollection<Place>).Remove(place);
						break;
					case Place.PlaceType.ACCOMMODATION:
						(AccommodationList.ItemsSource as ObservableCollection<Place>).Remove(place);
						break;
				}
				
			}
		}

		private void placeListView_Drop(object sender, DragEventArgs e)
		{
			ListView listView = (ListView)sender;
			ListView sourceListView = (ListView)e.Data.GetData("SourceListView");
			if (listView == sourceListView) {
				return;
			}

			if (e.Data.GetDataPresent("Place"))
			{
				Place place = e.Data.GetData("Place") as Place;


				switch (place.Type)
				{
					case Place.PlaceType.ATRACTION:
						(AttractionsList.ItemsSource as ObservableCollection<Place>).Add(place);
						AttractionsTabItem.IsSelected = true;
						break;
					case Place.PlaceType.RESTAURANT:
						(RestaurantsList.ItemsSource as ObservableCollection<Place>).Add(place);
						RestaurantsTabItem.IsSelected = true;
						break;
					case Place.PlaceType.ACCOMMODATION:
						(AccommodationList.ItemsSource as ObservableCollection<Place>).Add(place);
						AccommmodationTabItem.IsSelected = true;
						break;
				}
				(RearrangeListView.ItemsSource as ObservableCollection<Place>).Remove(place);

			}
		}

		private void NextButton_Click(object sender, RoutedEventArgs e)
		{
			ObservableCollection<Place> rearrangedPlaces = RearrangeListView.ItemsSource as ObservableCollection<Place>;
			ObservableCollection<ArrangementStep> steps = new ObservableCollection<ArrangementStep>();

			for(int i = 0; i < rearrangedPlaces.Count-1; i++)
			{
				ArrangementStep step = new ArrangementStep();
				step.StartPlace = rearrangedPlaces[i];
				step.EndPlace = rearrangedPlaces[i+1];

				steps.Add(step);
			}

			TransportListView.ItemsSource = steps;
			lastRearrengement = rearrangedPlaces;
			RearrangeListView.ItemsSource = new ObservableCollection<Place>();
			NextButton.IsEnabled = false;
			NextButton.ToolTip = (string)Application.Current.FindResource("String.Add2Places");
			BackButton.IsEnabled = true;

		}


		private void ListView_ItemClick(object sender, SelectionChangedEventArgs e)
		{
			// Cast the clicked item to the appropriate type
			ListView listView = (ListView)sender;
			selectedItem = (ArrangementStep)listView.SelectedItem;
			selectedListViewItem = listView.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListViewItem;

			PlaneToggle.IsEnabled = true;
			PlaneToggle.ToolTip = (string)Application.Current.FindResource("String.TransportPlane");
			TrainToggle.IsEnabled = true;
			TrainToggle.ToolTip = (string)Application.Current.FindResource("String.TransportTrain");
			BusToggle.IsEnabled = true;
			BusToggle.ToolTip = (string)Application.Current.FindResource("String.TransportBus");
			FootToggle.IsEnabled = true;
			FootToggle.ToolTip = (string)Application.Current.FindResource("String.TransportSelf");

		}

		private void OnBackClick(object sender, RoutedEventArgs e) => Parent.MainFrame.Content = new ArrangementsPage(Parent);

		private void PlaneToggle_Checked(object sender, RoutedEventArgs e)
		{
			TrainToggle.IsChecked = false;
			BusToggle.IsChecked = false;
			FootToggle.IsChecked = false;

			selectedItem.TransportationType = ArrangementStep.TransportType.PLANE;
			Image nestedImage = FindVisualChild<Image>(selectedListViewItem);
			nestedImage.Source = new BitmapImage(new Uri("../Resources/Images/plane.png", UriKind.Relative));

		}

		private void TrainToggle_Checked(object sender, RoutedEventArgs e)
		{
			PlaneToggle.IsChecked = false;
			BusToggle.IsChecked = false;
			FootToggle.IsChecked = false;

			selectedItem.TransportationType = ArrangementStep.TransportType.TRAIN;
			Image nestedImage = FindVisualChild<Image>(selectedListViewItem);
			nestedImage.Source = new BitmapImage(new Uri("../Resources/Images/train.png", UriKind.Relative));
		}

		private void BusToggle_Checked(object sender, RoutedEventArgs e)
		{
			PlaneToggle.IsChecked = false;
			TrainToggle.IsChecked = false;
			FootToggle.IsChecked = false;

			selectedItem.TransportationType = ArrangementStep.TransportType.BUS;
			Image nestedImage = FindVisualChild<Image>(selectedListViewItem);
			nestedImage.Source = new BitmapImage(new Uri("../Resources/Images/bus.png", UriKind.Relative));

		}

		private void FootToggle_Checked(object sender, RoutedEventArgs e)
		{
			PlaneToggle.IsChecked = false;
			TrainToggle.IsChecked = false;
			BusToggle.IsChecked = false;
			selectedItem.TransportationType = ArrangementStep.TransportType.FOOT;

			Image nestedImage = FindVisualChild<Image>(selectedListViewItem);
			nestedImage.Source = new BitmapImage(new Uri("../Resources/Images/walk.png", UriKind.Relative));

		}


		private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
		{
			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);

				if (child is T typedChild)
				{
					return typedChild;
				}

				T nestedChild = FindVisualChild<T>(child);
				if (nestedChild != null)
				{
					return nestedChild;
				}
			}

			return null;
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			BackButton.IsEnabled = false;
			NextButton.IsEnabled = true;
			NextButton.ToolTip = null;

			TransportListView.SelectedItem = null;

			PlaneToggle.ToolTip = (string)Application.Current.FindResource("String.ChooseStep");
			PlaneToggle.IsChecked = false;
			PlaneToggle.IsEnabled = false;

			TrainToggle.IsChecked = false;
			TrainToggle.ToolTip = (string)Application.Current.FindResource("String.ChooseStep");
			TrainToggle.IsEnabled = false;

			BusToggle.IsChecked = false;
			BusToggle.ToolTip = (string)Application.Current.FindResource("String.ChooseStep");
			BusToggle.IsEnabled = false;

			FootToggle.IsChecked = false;
			FootToggle.ToolTip = (string)Application.Current.FindResource("String.ChooseStep");
			FootToggle.IsEnabled = false;

			TransportListView.ItemsSource = new ObservableCollection<Place>();
			RearrangeListView.ItemsSource =lastRearrengement;
		}
	}
} 

