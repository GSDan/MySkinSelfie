using FFImageLoading.Forms;
using SkinSelfie.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class PhotoComparePage : ContentPage
    {
        List<PhotoCompareCell> top;
        List<PhotoCompareCell> bottom;

        public PhotoComparePage(ICollection<Photo> given)
        {
            Title = string.Format(AppResources.PhotoCompare_title, given.Count);

            RelativeLayout topLayout = new RelativeLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            RelativeLayout bottomLayout = new RelativeLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            top = new List<PhotoCompareCell>();
            bottom = new List<PhotoCompareCell>();

            for (int i = 0; i < given.Count; i++)
            {
                if (i % 2 == 0)
                {
                    top.Add(new PhotoCompareCell(given.ElementAt(i)));
                }
                else
                {
                    bottom.Add(new PhotoCompareCell(given.ElementAt(i)));
                }
            }

            for (int i = 0; i < top.Count; i++)
            {
                int index = i;

                topLayout.Children.Add(top[i],
                    Constraint.RelativeToParent((parent) =>
                    {
                        return (parent.Width / top.Count * index);
                    }),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent((parent) =>
                    {
                        return (parent.Width / top.Count);
                    }));
            }

            for (int i = 0; i < bottom.Count; i++)
            {
                int index = i;

                bottomLayout.Children.Add(bottom[i],
                    Constraint.RelativeToParent((parent) =>
                    {
                        return (parent.Width / bottom.Count * index);
                    }),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent((parent) =>
                    {
                        return (parent.Width / bottom.Count);
                    }));
            }

            RelativeLayout pageLayout = new RelativeLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            pageLayout.Children.Add(
                topLayout,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => {
                    return (parent.Width);
                }),
                Constraint.RelativeToParent((parent) => {
                    return (parent.Height / 2);
                })
            );

            pageLayout.Children.Add(
                bottomLayout,
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => {
                    return (parent.Height / 2);
                }),
                Constraint.RelativeToParent((parent) => {
                    return (parent.Width);
                }),
                Constraint.RelativeToParent((parent) => {
                    return (parent.Height / 2);
                })
            );

            Content = pageLayout;
        }

        //    protected override void OnAppearing()
        //    {
        //        base.OnAppearing();
        //        foreach (PhotoCompareCell cell in top)
        //        {
        //cell.photo.LoadImage (cell.image, false);
        //        }
        //        foreach (PhotoCompareCell cell in bottom)
        //        {
        //cell.photo.LoadImage (cell.image, false);
        //        }
        //    }

        //    protected override void OnDisappearing()
        //    {
        //        base.OnDisappearing();

        //        foreach(PhotoCompareCell cell in top)
        //        {
        //            cell.image.Source = null;
        //        }
        //        foreach (PhotoCompareCell cell in bottom)
        //        {
        //            cell.image.Source = null;
        //        }
        //    }
    }

    public class PhotoCompareCell : RelativeLayout
    {
        public Photo photo;
        public CachedImage image;

        public PhotoCompareCell(Photo given)
        {
            photo = given;

            image = new CachedImage()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Aspect = Aspect.AspectFill,
                BitmapOptimizations = true,
                DownsampleToViewSize = true,
                CacheDuration = TimeSpan.FromDays(30),
                Source = photo.GetReqUrl(false),
                RetryCount = 3,
                RetryDelay = 250
            };

            Children.Add(image, Constraint.Constant(0), Constraint.Constant(0), Constraint.RelativeToParent((parent) =>
            {
                return (parent.Width);
            }));

            BoxView shadeBox = new BoxView
            {
                BackgroundColor = Color.Black,
                Opacity = 0.45
            };
            Children.Add(
                shadeBox,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height * 0.12)
                );

            Label dateLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 17,
                TextColor = Color.White,
                Text = Utils.GetDateString(given.CreatedAt)
            };

            Children.Add(
                dateLabel,
                Constraint.Constant(8),
                Constraint.Constant(8)
            );

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;
            BackgroundColor = Color.Black;
            Padding = new Thickness(0);
        }
    }
}
