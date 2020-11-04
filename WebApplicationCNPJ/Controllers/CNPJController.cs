using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApplicationCNPJ.Context;
using WebApplicationCNPJ.Models;

namespace WebApplicationCNPJ.Controllers
{
    public class CNPJController : Controller
    {
        private readonly ILogger<CNPJController> _logger;

        private readonly CNPJDataContext _cnpjdataContext;

        private readonly int[] validationCNPJTable1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        private readonly int[] validationCNPJTable2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        private const string baseURL = "https://www.receitaws.com.br/";


        public CNPJController( ILogger<CNPJController> logger, CNPJDataContext cnpjdataContext )
        {
            this._logger = logger;
            this._cnpjdataContext = cnpjdataContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Search for information based in one CNPJ Code.
        /// </summary>
        /// <param name="cnpjcode">Code to get information from.</param>
        /// <returns>A page with the information given by the CNPJ.</returns>
        [HttpPost]
        public async Task<IActionResult> SearchCNPJAsync( string cnpjcode )
        {
            //Check regex once more.
            CNPJData data = new CNPJData()
            {
                CNPJCode = cnpjcode
            };

            //Is CNPJ correct?
            if( this.ValidateCNPJ( data.CNPJCode ) )
            {
                //Check if already exists in DB.
                CNPJData dbData = this._cnpjdataContext.CNPJData.SingleOrDefault( d => d.CNPJCode.Equals( cnpjcode ) );

                if(dbData == null )
                {

                    using( var client = new HttpClient() )
                    {
                        //Passing service base url  
                        client.BaseAddress = new Uri( CNPJController.baseURL );

                        client.DefaultRequestHeaders.Clear();
                        //Define request data format  
                        client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

                        //clean cnpj
                        string header = cnpjcode.Replace( ".", "" );
                        header = header.Replace( "/", "" );
                        header = header.Replace( "-", "" );

                        //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                        HttpResponseMessage Res = await client.GetAsync( "/v1/cnpj/" + header );

                        if( Res.IsSuccessStatusCode )
                        {
                            //Storing the response details recieved from web api   
                            var cnpjResponse = Res.Content.ReadAsStringAsync().Result;

                            //Deserializing the response recieved from web api and storing into the Employee list  
                            data = JsonConvert.DeserializeObject<CNPJData>( cnpjResponse );
                            data.CNPJCode = cnpjcode;


                        }
                        else
                            return BadRequest();
                    }

                    return View( "CNPJData", data );
                }
                else
                    return View( "CNPJData", dbData );
            }
            else
                return BadRequest();
        }

        /// <summary>
        /// Save CNPJ information in Database.
        /// </summary>
        /// <param name="model">CNPJ information.</param>
        /// <returns>To home page.</returns>
        [HttpPost]
        public IActionResult SaveCNPJData( CNPJData model )
        {
            if( ModelState.IsValid )
            {
                //Check if already exists in DB.
                CNPJData data = this._cnpjdataContext.CNPJData.SingleOrDefault( d =>d.CNPJCode.Equals( model.CNPJCode ) );

                if( data == null )
                {
                    //Generate a new radom id.
                    int id = new Random().Next( 1, int.MaxValue );

                    model.Id = id;

                    //Add and save the changes on the data base.
                    this._cnpjdataContext.Add( model );

                    this._cnpjdataContext.SaveChanges();
                }

                return RedirectToAction( "Index", "CNPJ" );
            }

            return BadRequest();
        }

        /// <summary>
        /// Get a view with all CNPJs information in the DataBase.
        /// </summary>
        /// <returns>A view with all CNPJs information in the DataBase.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCNPJDataList()
        {
            return View( "CNPJDataList", await this._cnpjdataContext.CNPJData.ToListAsync() );
        }

        /// <summary>
        /// Checks if a cnpj is valid.
        /// </summary>
        /// <param name="cnpjcode">The CNPJ to check.</param>
        /// <returns>True if valid, false if invalid.</returns>
        private bool ValidateCNPJ( string cnpjcode )
        {
            bool firstDigit = false;
            bool secondDigit = false;

            //Remove undesired chars.
            cnpjcode = cnpjcode.Replace( ".", "" );
            cnpjcode = cnpjcode.Replace( "/", "" );

            //Separate nums for validation
            string[] validation = cnpjcode.Split( "-" );

            //Multiply with validationCNPJTable1
            int[] table = new int[this.validationCNPJTable2.Length];

            for( int i = 0; i < this.validationCNPJTable1.Length; i++ )
            {
                table[i] = int.Parse( validation[0][i].ToString() ) * this.validationCNPJTable1[i];
            }

            //Check if first validator  digit is correct
            int divider = 11 - ( table.Sum() % 11 );

            if( table.Sum() % 11 < 2 && validation[1].ElementAt( 0 ) == '0' )
                firstDigit = true;
            else if( divider != int.Parse( validation[1][0].ToString() ) )
            {
                return false;
            }
            else
                firstDigit = true;


            //Multiply with validationCNPJTable2
            validation[0] += validation[1][0].ToString();

            for( int i = 0; i < this.validationCNPJTable2.Length; i++ )
            {
                table[i] = int.Parse( validation[0][i].ToString() ) * this.validationCNPJTable2[i];
            }

            //Check if second validator  digit is correct
            divider = 11 - ( table.Sum() % 11 );

            if( table.Sum() % 11 < 2 && validation[1].ElementAt( 1 ) == '0' )
                secondDigit = true;
            else if( divider != int.Parse( validation[1][1].ToString() ) )
            {
                return false;
            }
            else
                secondDigit = true;


            return firstDigit && secondDigit;
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public IActionResult Error()
        {
            return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
        }
    }
}
