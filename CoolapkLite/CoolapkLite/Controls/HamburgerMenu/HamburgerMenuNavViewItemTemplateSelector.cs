// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    internal class HamburgerMenuNavViewItemTemplateSelector : DataTemplateSelector
    {
        private readonly HamburgerMenu _hamburgerMenu;

        internal HamburgerMenuNavViewItemTemplateSelector(HamburgerMenu hamburgerMenu)
        {
            _hamburgerMenu = hamburgerMenu;
        }

        private DataTemplate SelectItemTemplate(object item)
        {
            if (_hamburgerMenu == null)
            {
                return TemplateFromItemTemplateSelector(item);
            }

            if (_hamburgerMenu.ItemsSource is IEnumerable<object> items && items.Contains(item))
            {
                return TemplateFromItemTemplateSelector(item);
            }

            if (_hamburgerMenu.OptionsItemTemplate != null)
            {
                if (_hamburgerMenu.OptionsItemsSource is IEnumerable<object> options && options.Contains(item))
                {
                    return TemplateFromOptionsItemTemplateSelector(item);
                }
            }

            return TemplateFromItemTemplateSelector(item);
        }

        private DataTemplate TemplateFromItemTemplateSelector(object item)
        {
            if (_hamburgerMenu.ItemTemplateSelector != null)
            {
                return _hamburgerMenu.ItemTemplateSelector.SelectTemplate(item);
            }

            return _hamburgerMenu.ItemTemplate;
        }

        private DataTemplate TemplateFromOptionsItemTemplateSelector(object item)
        {
            if (_hamburgerMenu.OptionsItemTemplateSelector != null)
            {
                return _hamburgerMenu.OptionsItemTemplateSelector.SelectTemplate(item);
            }

            return _hamburgerMenu.OptionsItemTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return SelectItemTemplate(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectItemTemplate(item);
        }
    }
}
