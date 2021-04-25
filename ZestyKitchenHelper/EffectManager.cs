using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using Utility;

namespace ZestyKitchenHelper
{
    public static class EffectManager
    {
        /// <summary>
        /// Method to handle what occurs when TouchEffect is active
        /// </summary>
        /// <param name="args"></param>
        /// <param name="itemIcon"></param>
        /// <param name="currentStorageName"></param>
        /// <param name="item"></param>
        /// <param name="grid"></param>
        public static void OnScreenTouch(TouchActionEventArgs args, ItemLayout itemIcon, string currentStorageName, Item item, Grid grid = null)
        {
            var itemBase = ContentManager.GetItemBase();
            itemIcon.TranslationY += args.Location.Y; itemIcon.TranslationX += args.Location.X;
            if (args.Type == TouchActionEventArgs.TouchActionType.Released)
            {
                itemIcon.TranslationX = 0;
                itemIcon.TranslationY = 0;
            }

            if (args.IsInContact)
            {
                foreach (var value in itemBase[currentStorageName].Values)
                {
                    foreach (ImageButton button in value.Keys)
                    {
                        button.RemoveEffect(typeof(ImageTint));
                    }
                }
                args.ContactView[0].ToggleEffects(new ImageTint() { tint = Color.FromRgba(100, 30, 30, 70) }, null);
                if (args.Type == TouchActionEventArgs.TouchActionType.Released)
                {
                    itemIcon.RemoveEffect(typeof(ScreenTouch));
                    itemIcon.iconImage.RemoveEffect(typeof(ScreenTouch));
                    args.ContactView[0].RemoveEffect(typeof(ImageTint));
                    itemBase[currentStorageName][args.ContactIndex[0]][args.ContactView[0] as ImageButton].Insert(0, itemIcon);
                    itemIcon.TranslationX += args.ContactView[0].TranslationX;
                    itemIcon.BindCabinetInfo(args.ContactView[0].TranslationX, args.ContactIndex[0], args.ContactView[0] as ImageButton, currentStorageName, ContentManager.GetStorageView);
                    CabinetAddPage.UpdateShelf(currentStorageName, itemIcon, args.ContactIndex[0], item);

                    if(grid != null)
                        GridManager.RemoveGridItem(grid, itemIcon);
                }
            }
            else if (args.ContactView != null) { args.ContactView[0].ToggleEffects(new ImageTint(), null); }

        }

        /// <summary>
        /// Update bounds in which TouchEffect is active
        /// </summary>
        /// <param name="view">View with TouchEffect attached</param>
        /// <param name="storageName">Name of storage the view belongs to</param>
        public static void UpdateScreenTouchBounds(View view, string storageName)
        {
            Console.WriteLine("AddView 119: touch check 1");
            var tryEffect = view.GetEffect(typeof(ScreenTouch)) as ScreenTouch;
            if (tryEffect != null)
            {
                tryEffect.ContactViews = ContentManager.GetContactViews(storageName);
            }
            else
            {
                Console.WriteLine("AddView 127: touch effect added");
                ScreenTouch touchEvent = new ScreenTouch() { ContactViews = ContentManager.GetContactViews(storageName) };
                touchEvent.OnTouchEvent += (obj, args) => OnScreenTouch(args, view as ItemLayout, storageName, (view as ItemLayout).ItemData);
                (view as ItemLayout).iconImage.Effects.Add(touchEvent);
            }
        }

        /// <summary>
        /// Update bounds in which TouchEffect is active
        /// </summary>
        /// <param name="grid">Item grid with TouchEffect attached views as children</param>
        /// <param name="storageName">Name of storage the view belongs to</param>
        public static void UpdateScreenTouchBounds(Grid grid, string storageName)
        {
            foreach (View child in grid.Children)
            {
                if (child.GetType() == typeof(ItemLayout))
                {
                    Console.WriteLine("AddView 119: touch check 1");
                    var tryEffect = child.GetEffect(typeof(ScreenTouch)) as ScreenTouch;
                    if (tryEffect != null)
                    {
                        tryEffect.ContactViews = ContentManager.GetContactViews(storageName);
                    }
                    else
                    {
                        Console.WriteLine("AddView 127: touch effect added");
                        ScreenTouch touchEvent = new ScreenTouch() { ContactViews = ContentManager.GetContactViews(storageName) };
                        touchEvent.OnTouchEvent += (obj, args) => OnScreenTouch(args, child as ItemLayout, storageName, (child as ItemLayout).ItemData, grid);
                        (child as ItemLayout).iconImage.Effects.Add(touchEvent);
                    }
                }
            }
        }

        public static void ClearEffects(this VisualElement element)
        {
            element.Effects.Clear();
        }
        public static void RemoveEffect(this VisualElement element, Type effectType)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
            if (tryEffect.Any()) { element.Effects.Remove(tryEffect.FirstOrDefault()); }
        }
        public static void RemoveEffects(this IList<View> elements, Type effectType)
        {
            foreach (View element in elements)
            {
                var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
                if (tryEffect.Any()) { element.Effects.Remove(tryEffect.FirstOrDefault()); }
            }
        }
        public static Effect GetEffect(this VisualElement element, Type effectType)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
            if (tryEffect.Any())
            {
                return tryEffect.First();
            }
            return null;
        }

        public static void AddEffect(this VisualElement element, Effect effect)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effect.GetType());
            if (!tryEffect.Any())
            {
                element.Effects.Add(effect);
            }
        }
        public static void ToggleEffects(this VisualElement button, Effect effect, List<VisualElement> toggleElements)
        {
            if (button != null)
            {
                var tryEffect = button.Effects.Where(e => e.GetType() == effect.GetType());
                if (tryEffect.Any())
                {
                    button.Effects.Remove(tryEffect.FirstOrDefault());
                    if (toggleElements != null)
                        foreach (var element in toggleElements)
                        {
                            element.IsVisible = false;
                        }
                }
                else
                {
                    button.Effects.Add(effect);

                    if (toggleElements != null)
                        foreach (var element in toggleElements)
                        {
                            element.IsVisible = true;
                        }
                }
            }
        }
    }


}
