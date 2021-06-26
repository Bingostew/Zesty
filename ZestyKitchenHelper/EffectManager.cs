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
        public static void OnScreenTouch(TouchActionEventArgs args, ItemLayout itemIcon, string currentStorageName, Action<string, ItemLayout, int> updateShelf)
        {
            var itemStorage = ContentManager.GetSelectedStorage(currentStorageName);
            itemIcon.TranslationY += args.Location.Y; itemIcon.TranslationX += args.Location.X;
            if (args.Type == TouchActionEventArgs.TouchActionType.Released)
            {
                itemIcon.TranslationX = 0;
                itemIcon.TranslationY = 0;
            }

            if (args.IsInContact)
            {
                foreach (StorageCell cell in itemStorage.GetGridCells())
                {
                    cell.GetButton().RemoveEffect(typeof(ImageTint));
                }
                args.ContactView[0].AddEffect(new ImageTint() { tint = Color.FromRgba(100, 30, 30, 70), ImagePath = ContentManager.buttonTintImage });
                if (args.Type == TouchActionEventArgs.TouchActionType.Released)
                {
                    args.ContactView[0].RemoveEffect(typeof(ImageTint));
                   
                    itemIcon.SetStoragePointer(ContentManager.GetStorageView);
                    updateShelf(currentStorageName, itemIcon, args.ContactIndex[0]);
                }
            }
            else if (args.ContactView != null) { args.ContactView[0].ToggleEffects(new ImageTint() { tint = Color.FromRgba(100, 30, 30, 70), ImagePath = ContentManager.buttonTintImage }, null); }

        }

        /// <summary>
        /// Update bounds in which TouchEffect is active
        /// </summary>
        /// <param name="view">View with TouchEffect attached</param>
        /// <param name="storageName">Name of storage the view belongs to</param>
        public static void UpdateScreenTouchBounds(ItemLayout view, string storageName, Action<string, ItemLayout, int> updateShelf)
        {
            Console.WriteLine("Updated Screen Touch Bounds");
            var tryEffect = view.GetEffect(typeof(ScreenTouch)) as ScreenTouch;
            Console.WriteLine("EffectManager 58 already have effect  " + (tryEffect != null));
            ScreenTouch touchEvent = new ScreenTouch() { ContactView = ContentManager.GetSelectedStorage(storageName) };
            touchEvent.OnTouchEvent += (obj, args) => OnScreenTouch(args, view, storageName, updateShelf);
            view.iconImage.Effects.Add(touchEvent);

        }

        public static void ClearEffects(this VisualElement element)
        {
            element.Effects.Clear();
        }
        public static void RemoveEffect(this VisualElement element, Type effectType)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
            Console.WriteLine("Effect Manager 76 Removed Effect Length " + tryEffect.Count());
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
