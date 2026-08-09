using BodyTracker.Services;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading;

public sealed class MessengerStatusService
{
    private static readonly Lazy<MessengerStatusService> _instance =
        new(() => new MessengerStatusService());

    public static MessengerStatusService Instance => _instance.Value;

    private int _generalFailureVersion;
    private int _databaseErrorVersion;

    private MessengerStatusService()
    {
        RegisterMessages();
    }

    #region Properties

    public string GeneralErrorMessage { get; private set; } = string.Empty;
    public string DatabaseErrorMessage { get; private set; } = string.Empty;

    public bool DatabaseConnected { get; private set; }

    public string SqlConnectionStatusMessage =>
        DatabaseConnected ? "✅ OK" : "❌ NOK";

    public string SqlAdditionalConnectionStatusMessage { get; private set; } = string.Empty;

    public string ActualUser { get; private set; } = string.Empty;

    public string ActualDatabase { get; private set; } = string.Empty;

    public int GeneralErrorMessageVersion => _generalFailureVersion;

    public int DatabaseErrorMessageVersion => _databaseErrorVersion;

    #endregion

    private void RegisterMessages()
    {
        WeakReferenceMessenger.Default.Register<GeneralErrorMessage>(this, (r, m) =>
        {
            GeneralErrorMessage = m.Value ?? string.Empty;
            Interlocked.Increment(ref _generalFailureVersion);
        });

        WeakReferenceMessenger.Default.Register<DatabaseErrorMessage>(this, (r, m) =>
        {
            DatabaseErrorMessage = m.Value ?? string.Empty;
            Interlocked.Increment(ref _databaseErrorVersion);
        });

        WeakReferenceMessenger.Default.Register<DatabaseConnectionStateMessage>(this, (r, m) =>
        {
            DatabaseConnected = m.Value;
        });

        WeakReferenceMessenger.Default.Register<DatabaseAdditionalConnectionStateMessage>(this, (r, m) =>
        {
            SqlAdditionalConnectionStatusMessage = m.Value ?? string.Empty;
        });

        WeakReferenceMessenger.Default.Register<LoggedInDatabaseUserMessage>(this, (r, m) =>
        {
            ActualUser = m.Value ?? string.Empty;
        });

        WeakReferenceMessenger.Default.Register<UsedAcutalDatabaseNameMessage>(this, (r, m) =>
        {
            ActualDatabase = m.Value ?? string.Empty;
        });

        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            LastNavigationTarget = m.Target;
        });
    }

    public string? LastNavigationTarget { get; private set; }
}