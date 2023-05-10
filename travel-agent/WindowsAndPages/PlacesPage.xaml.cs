﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using travel_agent.Models;
using travel_agent.Services;

namespace travel_agent.WindowsAndPages
{
    public partial class PlacesPage : Page
    {
        private new readonly MainWindow Parent;
        private PlaceService PlaceService { get; set; }
        public List<Place> Places { get; set; }
        public PlacesPage(MainWindow parent)
        {
            InitializeComponent();
            Parent = parent;
            PlaceService = PlaceService.Instance;
            SetPlacesList();
            DataContext = this;
            if (Places.Count <= 1) SetupIfListEmpty();
            if (Parent.User.Role != Role.AGENT) PlacesItemsControl.Loaded += CollapseFirstItem;
        }

        private void CollapseFirstItem(object sender, RoutedEventArgs e)
        {
            var firstItemContainer = PlacesItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as UIElement;
            if (firstItemContainer != null) firstItemContainer.Visibility = Visibility.Collapsed;
            PlacesItemsControl.Loaded -= CollapseFirstItem;
        }

        private void SetPlacesList()
        {
            Places = new List<Place> {null};
            foreach (Place p in PlaceService.GetAll()) Places.Add(p);
        }

        private void SetupIfListEmpty()
        {
            if (Parent.User.Role == Role.AGENT)
            {
                PlacesList.Margin = new Thickness(20);
                AdvanceSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                AdvanceSearch.Visibility = Visibility.Collapsed;
                PlacesList.Visibility = Visibility.Collapsed;
                EmptyPlaceList.Visibility = Visibility.Visible;
            }
        }

        private void OnAddNewPlaceClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Parent.MainFrame.Content = new AddAndModifyPlacePage(Parent);
        }

        private void OnPlaceClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            object data = (sender as Grid).DataContext;
            if (Parent.User.Role == Role.AGENT) Parent.MainFrame.Content = new AddAndModifyPlacePage(Parent, data as Place);
            else Parent.MainFrame.Content = new ViewPlacePage(Parent, data as Place);
        }

        private void OnShowPopupClick(object sender, RoutedEventArgs e)
        {
            ShowPopupButton.Visibility = Visibility.Collapsed;
            HidePopupButton.Visibility = Visibility.Visible;
            AdvancedSearchPopup.IsOpen = true;
        }

        private void OnHidePopupClick(object sender, RoutedEventArgs e)
        {
            ShowPopupButton.Visibility = Visibility.Visible;
            HidePopupButton.Visibility = Visibility.Collapsed;
            AdvancedSearchPopup.IsOpen = false;
        }
    }

    public class FirstItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);
            if (index == 0) return (DataTemplate)itemsControl.FindResource("FirstItemTemplate");
            else return (DataTemplate)itemsControl.FindResource("DefaultItemTemplate");
        }
    }
}
