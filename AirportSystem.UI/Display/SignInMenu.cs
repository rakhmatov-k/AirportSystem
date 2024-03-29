﻿using AirportSystem.Data.Repositories;
using AirportSystem.Domain.Entities.Customer;
using AirportSystem.Service.Services;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace AirportSystem.UI.Display;

public class SignInMenu
{
    private readonly EmployeeRepository employeeRepository;
    private readonly CustomerRepository customerRepository;
    private readonly AircraftRepository aircraftRepository;
    private readonly FlightRepository flightRepository;
    private readonly FlightEmployeeRepository flightEmployeeRepository;
    private readonly TicketRepository ticketRepository;
    private readonly BookingRepository bookingRepository;

    private readonly CustomerService customerService;
    private readonly EmployeeService employeeService;
    private readonly AircraftService aircraftService;
    private readonly FlightService flightService;
    private readonly FlightEmployeeService flightEmployeeService;
    private readonly TicketService ticketService;
    private readonly BookingService bookingService;

    private readonly EmployeeMenu employeeMenu;
    private readonly AircraftMenu aircraftMenu;
    private readonly FlightMenu flightMenu;
    private readonly FlightEmployeesMenu flightEmployeesMenu;
    private readonly TicketMenu ticketMenu;
    private readonly BookingMenu bookingMenu;
    private readonly CustomerMenu customerMenu;

    private CustomerViewModel customer;
    public SignInMenu(EmployeeRepository employeeRepository, CustomerRepository customerRepository, AircraftRepository aircraftRepository, 
        FlightRepository flightRepository, FlightEmployeeRepository flightEmployeeRepository, TicketRepository ticketRepository,
        BookingRepository bookingRepository, CustomerService customerService, EmployeeService employeeService, AircraftService aircraftService,
        FlightService flightService, FlightEmployeeService flightEmployeeService, TicketService ticketService, BookingService bookingService)
    {
        this.employeeRepository = employeeRepository;
        this.customerRepository = customerRepository;  
        this.aircraftRepository = aircraftRepository;
        this.flightRepository = flightRepository;
        this.flightEmployeeRepository = flightEmployeeRepository;
        this.ticketRepository = ticketRepository;
        this.bookingRepository = bookingRepository;

        this.customerService = customerService;
        this.employeeService = employeeService;
        this.aircraftService = aircraftService;
        this.flightService = flightService;
        this.flightEmployeeService = flightEmployeeService;
        this.ticketService = ticketService;
        this.bookingService = bookingService;

        employeeMenu = new EmployeeMenu(employeeService);
        aircraftMenu = new AircraftMenu(aircraftService);
        flightMenu = new FlightMenu(flightService,aircraftService);
        flightEmployeesMenu = new FlightEmployeesMenu(flightEmployeeService,flightService,employeeService);
        ticketMenu = new TicketMenu(ticketService, flightService);
        bookingMenu = new BookingMenu(bookingService,ticketService,flightService);
        customerMenu = new CustomerMenu(customerService);
    }

    public async Task SignInAsync()
    {
        bool circle = true;
        var somebody = await SecurityCheck();
        if (somebody == "admin")
        {
            var options = new string[] { "Employee", "Aircraft", "Flight", "FlightEmployees", "Ticket","Users", "[red]Back[/]" };
            var title = "-- AdminMenu --";

            while (circle)
            {
                AnsiConsole.Clear();
                var selection = Selection.SelectionMenu(title, options);
                switch (selection)
                {
                    case "Employee":
                        await employeeMenu.DisplayAsync();
                        break;
                    case "Aircraft":
                        await aircraftMenu.DisplayAsync();
                        break;
                    case "Flight":
                        await flightMenu.DisplayAsync();
                        break;
                    case "FlightEmployees":
                        await flightEmployeesMenu.DisplayAsync();
                        break;
                    case "Ticket":
                        await ticketMenu.DisplayAsync();
                        break;
                    case "Users":
                        await customerMenu.GetAllAsync();
                        break;
                    case "[red]Back[/]":
                        circle = false;
                        break;
                }
            }
        }else if(somebody == "user")
        {
            var options = new string[] { "Booking Ticket", "Return Ticket", "GetById", "GetAll", "MyAccount", "Deposit", "UpdateAccount", "Delete", "[red]Back[/]" };
            var title = "-- UserMenu --";

            while (circle)
            {
                AnsiConsole.Clear();
                var selection = Selection.SelectionMenu(title, options);
                switch (selection)
                {
                    case "Booking Ticket":
                        await bookingMenu.BookingAsync(customer.Id);
                        break;
                    case "Return Ticket":
                        await bookingMenu.ReturnTicketAsync(customer.Id);
                        break;
                    case "GetById":
                        await bookingMenu.GetByIdAsync(customer.Id);
                        break;
                    case "GetAll":
                        await bookingMenu.GetAllAsync(customer.Id);
                        break;
                    case "MyAccount":
                        await customerMenu.GetByIdAsync(customer.Id);
                        break;
                    case "Deposit":
                        await customerMenu.DepositAsync(customer.Id);
                        break;
                    case "UpdateAccount":
                        await customerMenu.UpdateAsync(customer.Id);
                        break;
                    case "Delete":
                        await customerMenu.DeleteAsync(customer.Id);
                        circle = false;
                        break;
                    case "[red]Back[/]":
                        circle = false;
                        break;
                }
            }
        }
        else
        {
            circle = false;
        }
    }

    public async Task<string> SecurityCheck()
    {
        AnsiConsole.Clear();
        string result = "another";
        string email = AnsiConsole.Ask<string>("Email [blue](email@gmail.com):[/]");
        while (!Regex.IsMatch(email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]{3,}$"))
        {
            AnsiConsole.MarkupLine("[red]Was entered in the wrong format .Try again![/]");
            email = AnsiConsole.Ask<string>("Email [blue](email@gmail.com):[/]");
        }

        string password = AnsiConsole.Prompt<string>(new TextPrompt<string>("Enter your password:").Secret());
        while (!Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$"))
        {
            AnsiConsole.MarkupLine("[red]Invalid input.[/]");
            password = AnsiConsole.Prompt<string>(new TextPrompt<string>("Enter your password:").Secret());
        }

        if (email == "adminInfo@gmail.com" && password == "admin123")
        {
            result = "admin";
        }
        else
        {
            try
            {
                customer = await customerService.SecurityCheckAsync(email, password);
                result = "user";
            }
            catch (Exception ex)
            {
                AnsiConsole.Markup($"[red]{ex.Message}[/]\n");
                Console.WriteLine("Enter any keyword to continue");
                Console.ReadKey();
                Console.Clear();
                return result;
            }
        }
        return result;
    }
}