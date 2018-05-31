using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BangumiSU.ApiClients;
using BangumiSU.SharedCode;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BangumiSU.Pages.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private AccountClient accountClient;

        public LoginDialog(AccountClient accountClient)
        {
            this.InitializeComponent();
            this.accountClient = accountClient;
        }

        private void Cancel_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void Login_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                TbInfo.Text = "";
                IsPrimaryButtonEnabled = false;
                var msg = await accountClient.LoginAsync(TbUser.Text, PbPwd.Password);
                if (!msg.IsEmpty())
                {
                    TbInfo.Text = msg;
                    args.Cancel = true;
                }
            }
            catch (Exception e)
            {
                TbInfo.Text = e.Message;
                args.Cancel = true;
            }
            finally
            {
                IsPrimaryButtonEnabled = true;
            }
        }
    }
}
