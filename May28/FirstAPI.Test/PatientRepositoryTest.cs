using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using FirstAPI.Models.DTOs;
using System.Text;



namespace FirstAPI.Test;

public class PatientRepositoryTest
{
    private ClinicContext _context;
    private IRepository<int, Patient> _patientRepository;
    private IRepository<string, User> _userRepository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

        _context = new ClinicContext(options);
        _patientRepository = new PatientRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Test]
    public async Task AddPatientPassTest()
    {
        //Arrange 
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Patient"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var patient = new Patient()
        {
            Name = "test",
            Age = 20,
            Email = email,
            Phone = "1234567",
        };

        //Action 
        var result = await _patientRepository.Add(patient);

        //Assert 
        Assert.That(result, Is.Not.Null);

    }

    [Test]
    public async Task UpdatePatientPassTest()
    {
        //Arrange
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Patient"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var patient = new Patient()
        {
            Name = "test",
            Age = 20,
            Email = email,
            Phone = "1234567",
        };
        var updatePatient = await _patientRepository.Add(patient);
        updatePatient.Age = 30;

        //Action 
        var result = await _patientRepository.Update(updatePatient.Id, updatePatient);

        //Assert
        Assert.That(result.Age, Is.EqualTo(30));

    }

    [Test]
    public async Task DeletePatientPassTest()
    {
        //Arrange
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Patient"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var patient = new Patient()
        {
            Name = "test",
            Age = 20,
            Email = email,
            Phone = "1234567",
        };
        var addedPatient = await _patientRepository.Add(patient);

        //Action
        var result = await _patientRepository.Delete(addedPatient.Id);

        //Assert 
        Assert.That(async () => await _patientRepository.GetByID(result.Id),
            Throws.TypeOf<Exception>());
        
    }

    [Test]
    public async Task GetPatientPassTest()
    {
        //Arrange
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Patient"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var patient = new Patient()
        {
            Name = "test",
            Age = 20,
            Email = email,
            Phone = "1234567",
        };
        var addedPatient = await _patientRepository.Add(patient);

        //Action 
        var result = await _patientRepository.GetByID(addedPatient.Id);

        //Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAllPatientsPassTest()
    {
        var email = "test@gmail.com";
        var password = Encoding.UTF8.GetBytes("testpassword");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User()
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Patient"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        var patient = new Patient()
        {
            Name = "test",
            Age = 20,
            Email = email,
            Phone = "1234567",
        };

        var email2 = "test2@gmail.com";
        var password2 = Encoding.UTF8.GetBytes("test2password");
        var key2 = Guid.NewGuid().ToByteArray();
        var user2 = new User()
        {
            Username = email2,
            Password = password2,
            HashKey = key2,
            Role = "Patient"
        };
        _context.Add(user2);
        await _context.SaveChangesAsync();

        var patient2 = new Patient()
        {
            Name = "test2",
            Age = 20,
            Email = email2,
            Phone = "1234567",
        };
        await _patientRepository.Add(patient);
        await _patientRepository.Add(patient2);

        //Action
        var result = await _patientRepository.GetAll();

        //Assert
        Assert.That(result,Is.Not.Null);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
}