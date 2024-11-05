using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using AvaloniaApplication13.Helper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Ursa.Controls;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace AvaloniaApplication13.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly HubConnection _connection;

    public MainViewModel()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/chatHub")
            .AddMessagePackProtocol()
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += OnConnectionOnClosedAsync;
        _connection.On<string, string>("ReceiveMessage", OnReceiveMessageHandlerAsync);
    }

    [ObservableProperty] private bool _connected;
    [ObservableProperty] private string? _title;
    [ObservableProperty] private string? _user;
    [ObservableProperty] private string? _content;
    [ObservableProperty] private bool _autoClearText;

    private async Task OnReceiveMessageHandlerAsync(string user, string message)
    {
        await MessageHelper.ShowAsync(new Notification(
            title: "新消息",
            message: $"{user}-{message}",
            type: NotificationType.Information));
    }

    private async Task OnConnectionOnClosedAsync(Exception? arg)
    {
        Connected = false;
        await MessageHelper.ShowErrorAsync($"断开连接-{arg?.Message}");
    }

    [RelayCommand]
    private async Task OnConnectionAsync()
    {
        if (Connected)
        {
            await MessageBox.ShowOverlayAsync("已经连接", "提示", icon: MessageBoxIcon.Error,
                button: MessageBoxButton.OK);
            return;
        }

        Connected = false;
        Title = "正在连接";

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception e)
        {
            await MessageHelper.ShowErrorAsync($"连接出错!{e.Message}");
            return;
        }

        Title = "连接成功";
        Connected = true;
        await MessageHelper.ShowInfoAsync("连接成功.");
    }

    [RelayCommand]
    private async Task OnSendMessage()
    {
        if (!Connected)
        {
            await MessageBox.ShowOverlayAsync("请先连接.", "提示",
                icon: MessageBoxIcon.Error,
                button: MessageBoxButton.OK);
            return;
        }

        if (string.IsNullOrWhiteSpace(User) || string.IsNullOrWhiteSpace(Content))
        {
            await MessageBox.ShowOverlayAsync("输入错误，请重试.", "提示", icon: MessageBoxIcon.Error,
                button: MessageBoxButton.OK);
            return;
        }

        try
        {
            await _connection.SendAsync("SendMessage", User, Content);
        }
        catch (Exception e)
        {
            await MessageHelper.ShowErrorAsync($"发送出错!{e.Message}");
        }

        if (AutoClearText)
        {
            Content = string.Empty;
        }
    }
}