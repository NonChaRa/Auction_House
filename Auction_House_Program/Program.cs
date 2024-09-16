using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace AuctionHouse
{
    public class User 
    {
        public int UserId { get; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string HomeAddress { get; set; }
        public List<ProductData> AdvertisedProducts { get; set; }
        public List<PurchaseData> PurchasedProducts { get; set; }
        private int currentProductNumber = 1;
        public User(int userId, string name, string email, string password) 
        {
            UserId = userId;
            Name = name;
            Email = email;
            Password = password;
            AdvertisedProducts = new List<ProductData>();
            PurchasedProducts = new List<PurchaseData>();
        }
        public int GetCurrentProductNumber()
        {
            return currentProductNumber;
        }

        public void IncrementProductNumber()
        {
            currentProductNumber++;
        }
        public void AddPurchasedProduct(PurchaseData purchase)
        {
            PurchasedProducts.Add(purchase);
        }
        
    }
    public class ProductData
    {
        public static List<ProductData> AdvertisedProducts = new List<ProductData>();
        public int Number {get; set;}
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal SalePrice { get; set; }
        public int AdvertiserUserId { get; set; }
        public BidData Bid { get; set; }
        public PickupWindow PickupWindow { get; set; }
        public ProductData()
        {
            ProductName = string.Empty;
            ProductDescription = string.Empty;
            PickupWindow = new PickupWindow { StartTime = DateTime.MinValue, EndTime = DateTime.MinValue };
        }
    
        public void AddProduct(User user, ProductData product) // count the number of products added
        {
            AdvertisedProducts = AdvertisedProducts.OrderBy(p => p.ProductName).ToList();
            product.Number = user.GetCurrentProductNumber();
            user.IncrementProductNumber();
            product.AdvertiserUserId = user.UserId;
            ProductData.AdvertisedProducts.Add(product);
        }
        public static bool ValidCurrency(string input, out decimal currencyAmount)
        {
            currencyAmount = 0;

            // Check if the input is null or empty
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            // Remove whitespaces
            input = input.Trim();

            if (input.Length > 0 && input[0] == '$')
            {
                input = input.Substring(1);
            }
            string[] parts = input.Split('.');

            if (parts.Length != 2)
            {
                return false;
            }
            if (decimal.TryParse(input, NumberStyles.Currency, CultureInfo.InvariantCulture, out currencyAmount))
            {
                return true;
            }
            return false;
        }
    }
    public class BidData
    {
        public string BidderName { get; set; }
        public string BidderEmail { get; set; }
        public double BidPrice { get; set; }
        public BidData()
        {
            BidderName = string.Empty;
            BidderEmail = string.Empty;
        }
    }
    public class PurchaseData
    {
        public string SellerEmail { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ListPrice { get; set; }
        public decimal AmountPaid { get; set; }
        public string DeliveryOptionSynopsis { get; set; }
        public PurchaseData()
        {
            SellerEmail = string.Empty;
            ProductName = string.Empty;
            ProductDescription = string.Empty;
            DeliveryOptionSynopsis = string.Empty;
        }
    }
        public class PickupWindow
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public PickupWindow()
        {
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
        }

        public static bool ValidatePickupWindow(DateTime startTime, DateTime endTime)
        {
            // Start time must be at least one hour later than the current system time
            // End time must be at least one hour after the start time
            DateTime currentSystemTime = DateTime.Now;
            return startTime > currentSystemTime.AddHours(1) && endTime > startTime.AddHours(1);
        }
    }
    

    public interface IMenuOption
    {
        void Execute(User signedInUser);
    }

    public class AdvertiseProduct : IMenuOption
    {
        public void Execute(User signedInUser)
        {
            Console.WriteLine("+-------------------+");
            Console.WriteLine("| Advertise Product |");
            Console.WriteLine("+-------------------+");
            Console.WriteLine("\nProduct name:");
            string productName;
            do
            {
                productName = Console.ReadLine()?.Trim();
            } while (string.IsNullOrWhiteSpace(productName));
            Console.WriteLine("Product description:");
            string productDescription;
            do
            {
                productDescription = Console.ReadLine()?.Trim();
            } while (string.IsNullOrWhiteSpace(productDescription) || productDescription.Equals(productName, StringComparison.OrdinalIgnoreCase));
            decimal salePrice;

            while (true)
            {
                Console.WriteLine("Sale price:");
                string salePriceInput = Console.ReadLine();
                if (ProductData.ValidCurrency(salePriceInput, out salePrice))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid sale price.");
                }
            }
            ProductData newProduct = new ProductData
            {
                ProductName = productName,
                ProductDescription = productDescription,
                SalePrice = salePrice
            };

            newProduct.AddProduct(signedInUser, newProduct);
            signedInUser.AdvertisedProducts.Add(newProduct);
            Console.WriteLine($"\nSuccessfully created advertisement:\n\t{newProduct.ProductName} - {newProduct.ProductDescription} ${newProduct.SalePrice}");
            Console.WriteLine("\nReturning to the Client Menu.");
            ProductList.AssignProductNumbers(signedInUser.AdvertisedProducts);
            ClientMenu.ShowMenu(signedInUser);
        }
    }
    public class ProductList : IMenuOption
    {
        private User currentUser;

        public ProductList(User user)
        {
            currentUser = user;
        }
        
        public void Execute(User signedInUser)
        {
            Console.WriteLine("+--------------+");
            Console.WriteLine("| Product List |");
            Console.WriteLine("+--------------+\n");

            DisplayProducts(currentUser, currentUser.AdvertisedProducts);
            Console.WriteLine("\nReturning to Client Menu.");
            ClientMenu.ShowMenu(signedInUser);
        }
        public static void AssignProductNumbers(List<ProductData> products)
        {
            // Reassign product numbers in ascending order
            for (int i = 0; i < products.Count; i++)
            {
                products[i].Number = i + 1;
            }
        }
        public static void DisplayProducts(User currentUser, IEnumerable<ProductData> products, bool showCompleteMessage = true)
        {
            if (products.Any())
            {
                Console.WriteLine("Number\tName\tDescription\tPrice\tBidder name\tBidder email\tBid price\t");
                var sortedProducts = products.OrderBy(p => p.ProductName);
                AssignProductNumbers(sortedProducts.ToList());
                foreach (var product in sortedProducts)
                {
                    Console.WriteLine($"{product.Number}\t{product.ProductName}\t{product.ProductDescription}\t${product.SalePrice}\t");

                    if (product.Bid != null)
                    {
                        Console.Write($"{product.Bid.BidderName}\t\t{product.Bid.BidderEmail}\t{product.Bid.BidPrice}");
                    }
                    else
                    {
                        Console.Write("-\t\t-\t\t-");
                    }
                    Console.WriteLine("\t");
                }
                if (showCompleteMessage)
                {
                    Console.WriteLine("\nProduct list complete.\n");
                }
            }
            else
            {
                Console.WriteLine("You have no advertised products at this time.\n");
            }
            
        }

    }

    public class SearchProduct : IMenuOption 
    {
        public void Execute(User signedInUser)
        {
            Console.WriteLine("+----------------+");
            Console.WriteLine("| Product Search |");
            Console.WriteLine("+----------------+");
            Console.WriteLine();
            string searchPhrase;
            do
            {
                Console.WriteLine("Search phrase (ALL to match all products): ");
                searchPhrase = Console.ReadLine()?.Trim();
            } while (string.IsNullOrWhiteSpace(searchPhrase));
            List<ProductData> matchingProducts;
    
            if (string.Equals(searchPhrase, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                matchingProducts = ProductData.AdvertisedProducts
                    .Where(product => product.AdvertiserUserId != signedInUser.UserId)
                    .ToList();
            }
            else
            {
                matchingProducts = ProductData.AdvertisedProducts
                    .Where(product => (product.ProductName.Contains(searchPhrase, StringComparison.OrdinalIgnoreCase) || 
                    product.ProductDescription.Contains(searchPhrase, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            if (matchingProducts.Count > 0)
            {
                ProductList.DisplayProducts(signedInUser, matchingProducts, false);
            }
            else
            {
                Console.WriteLine("\nNo products match the search phrase.");
                Console.WriteLine("\nReturning to Client Menu.\n");
                ClientMenu.ShowMenu(signedInUser);
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Product search complete.\n");
            Console.WriteLine("Would you like to place a bid (yes or no)?");
            string ? bidCondition = Console.ReadLine();
            if (bidCondition.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Which product would you like to bid for (1..1)?");
                string ? productNumber = Console.ReadLine();
                    if (int.TryParse(productNumber, out int convertedProductNumber))
                    {
                        ProductData selectedProduct = ProductData.AdvertisedProducts.FirstOrDefault(product => product.Number == convertedProductNumber);

                        if (selectedProduct != null)
                        {
                            Console.WriteLine($"You selected: {selectedProduct.ProductName} - {selectedProduct.ProductDescription} - ${selectedProduct.SalePrice}");

                            while (true)
                            {
                                Console.WriteLine("Please enter the bid amount:");
                                string bidAmountInput = Console.ReadLine();

                                if (ProductData.ValidCurrency(bidAmountInput, out decimal bidAmount))
                                {
                                    if (selectedProduct.Bid == null || bidAmount > (decimal)selectedProduct.Bid.BidPrice)
                                    {
                                        selectedProduct.Bid = new BidData
                                        {
                                            BidderName = signedInUser.Name,
                                            BidderEmail = signedInUser.Email,
                                            BidPrice = (double)bidAmount
                                        };

                                        break; 
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Your bid of ${bidAmount} is not higher than the current highest bid of ${selectedProduct.Bid.BidPrice}. Bid rejected.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid bid amount. Please enter a valid currency amount.");
                                }
                            }
                            Console.WriteLine("How would you like to receive the item (collect or deliver)?");
                            string ? productDeliveryOption = Console.ReadLine();
                            if (productDeliveryOption.Equals("collect", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Delivery window start (any time after " + DateTime.Now.AddHours(1) + "):");
                                DateTime startTime;
                                while (!DateTime.TryParse(Console.ReadLine(), out startTime) || startTime <= DateTime.Now.AddHours(1))
                                {
                                    Console.WriteLine("Invalid input. Please enter a valid date and time after " + DateTime.Now.AddHours(1) + ".");
                                }

                                Console.WriteLine("Delivery window end (any time after " + startTime.AddHours(1) + "):");
                                DateTime endTime;
                                while (!DateTime.TryParse(Console.ReadLine(), out endTime) || endTime <= startTime.AddHours(1))
                                {
                                    Console.WriteLine("Invalid input. Please enter a valid date and time after " + startTime.AddHours(1) + ".");
                                }

                                selectedProduct.PickupWindow = new PickupWindow
                                {
                                    StartTime = startTime,
                                    EndTime = endTime
                                };

                                Console.WriteLine($"Bid successfully placed:\n${selectedProduct.SalePrice} {signedInUser.Name} {signedInUser.Email} Pick up between\n{startTime} and {endTime}");
                            }
                            else if (productDeliveryOption.Equals("deliver", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Deliver to your home address (yes or no)?");
                                string ? useHomeAddress = Console.ReadLine();
                                if (useHomeAddress.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!string.IsNullOrEmpty(signedInUser.HomeAddress))
                                    {
                                        // Use the user's home address for delivery
                                        Console.WriteLine($"Bid successfully placed:\n${selectedProduct.SalePrice} {signedInUser.Name} {signedInUser.Email} Home delivery to {signedInUser.HomeAddress}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("No home address found.");
                                    }
                                }
                                else if (useHomeAddress.Equals("no", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Prompt the user for a delivery address
                                    Console.WriteLine("Please provide a delivery address:");
                                    string ? deliveryAddress = Console.ReadLine();

                                    if (!string.IsNullOrEmpty(deliveryAddress))
                                    {
                                        Console.WriteLine($"Bid successfully placed:\n${selectedProduct.SalePrice}\t{signedInUser.Name} {signedInUser.Email} Deliver to {deliveryAddress}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid delivery address. Bid rejected.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid product number. Please enter a valid product number.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid product number.");
                    }
            }
            else if (bidCondition.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Bid postponed.");
            }
            Console.WriteLine("Returning to Client Menu.");
            ClientMenu.ShowMenu(signedInUser);
        }
        private bool CurrentUserProduct(User user, ProductData product)
        {
            return user.AdvertisedProducts.Any(p => p.Number == product.Number);
        }
    }
   public class DisplayProductBids : IMenuOption
    {
        public void Execute(User signedInUser)
        {
            Console.WriteLine("+--------------+");
            Console.WriteLine("| Product Bids |");
            Console.WriteLine("+--------------+\n");

            bool hasBiddedProducts = false;
            var biddedProducts = signedInUser.AdvertisedProducts.Where(product => product.Bid != null).ToList();
            if (biddedProducts.Count > 0)
            {
                Console.WriteLine("Number\tName\tDescription\tPrice\tBidder Name\tBidder Email\tBid Price\t");

                var sortedProducts = biddedProducts.OrderBy(p => p.ProductName).ThenBy(p => p.ProductDescription).ThenBy(p => p.SalePrice);

                foreach (var product in sortedProducts)
                {
                    if (product != null) 
                    {
                        DisplayProduct(product);
                    }
                    else 
                    {
                        Console.WriteLine("product is Null.");
                    }
                }
                hasBiddedProducts = true;
                Console.WriteLine("Do you want to sell a product? (yes or no)");
                string sellResponse = Console.ReadLine();

                if (sellResponse.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    SellProduct(signedInUser, biddedProducts);
                }
                else if (sellResponse.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Sale postponed.");
                }
            }

            if (!hasBiddedProducts)
            {
                Console.WriteLine("You have no products with bids at this time.");
            }
            Console.WriteLine("Returning to Client Menu.");
            ClientMenu.ShowMenu(signedInUser);
        }
        private static void DisplayProduct(ProductData product)
        {
            Console.WriteLine($"{product.Number}\t{product.ProductName}\t{product.ProductDescription}\t${product.SalePrice}\t");

            if (product.Bid != null)
            {
                Console.WriteLine($"{product.Bid.BidderName}\t\t{product.Bid.BidderEmail}\t{product.Bid.BidPrice}\t\n");
            }
            else
            {
                Console.WriteLine("-\t\t-\t\t-\t\n");
            }
        }
        private void SellProduct(User signedInUser, List<ProductData> biddedProducts)
        {
            Console.WriteLine("Item number (1..1)");
            if (int.TryParse(Console.ReadLine(), out int selectedRow))
            {
                if (selectedRow >= 1 && selectedRow <= biddedProducts.Count)
                {
                    ProductData selectedProduct = biddedProducts[selectedRow - 1];
                    Console.WriteLine($"Product {selectedProduct.ProductName} sold to {selectedProduct.Bid.BidderName} for ${selectedProduct.Bid.BidPrice}");
                    Console.WriteLine($"\tCollection arrangement: Pick up between {selectedProduct.PickupWindow.StartTime} and {selectedProduct.PickupWindow.EndTime}");
                }
                else
                {
                    Console.WriteLine("Invalid row number. Please enter a valid row number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid row number.");
            }
        }
    }
    public class PurchasedProductsList : IMenuOption
    {
        public void Execute (User signedInUser)
        {
            Console.WriteLine("+--------------------+");
            Console.WriteLine("| Purchased Products |");
            Console.WriteLine("+--------------------+");
            if (signedInUser.PurchasedProducts.Any())
            {
                Console.WriteLine("Number\tSeller Email\tName\tDescription\tList Price\tAmount Paid\tDelivery Option");
                var sortedPurchasedProducts = signedInUser.PurchasedProducts.OrderBy(p => p.ProductName)
                .ThenBy(p => p.ProductDescription)
                .ThenBy(p => p.ListPrice);
                int rowNumber = 1;
                foreach (var purchase in sortedPurchasedProducts)
                {
                    Console.WriteLine($"{rowNumber}\t{purchase.SellerEmail}\t{purchase.ProductName}\t{purchase.ProductDescription}\t${purchase.ListPrice}\t${purchase.AmountPaid}\t{purchase.DeliveryOptionSynopsis}");
                    rowNumber++;
                }
                Console.WriteLine("\nPurchase list complete.");
            }
            else
            {
                Console.WriteLine("You have not purchased any items so far.");
            }
            Console.WriteLine("Returning to Client Menu.");
            ClientMenu.ShowMenu(signedInUser);
        }
    }


    public class UserAddressMenu : IMenuOption
    {
        public void Execute(User signedInUser)
        {
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("| home delivery address |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("\nUnit number (leave blank if none):");
            string unitNumInput = Console.ReadLine();
            int ? unitNum = null;
            if (!string.IsNullOrWhiteSpace(unitNumInput) && int.TryParse(unitNumInput, out int parsedUnitNum) && parsedUnitNum > 0)
            {
                unitNum = parsedUnitNum;
            }
            Console.WriteLine("Street number:");
            int streetNum;
            while (!int.TryParse(Console.ReadLine(), out streetNum) || streetNum <= 0)
            {
                Console.WriteLine("Invalid input. Please enter a valid positive nonzero street number.");
            }

            Console.WriteLine("Street name:");
            string streetName = Console.ReadLine();

            Console.WriteLine("Street type (St, Rd, Ave, Blvd, Dr, Ln, Ct, Pl, Ter, Way):");
            string streetType = Console.ReadLine();

            if (!StreeTypeValidation(streetType))
            {
                Console.WriteLine("Invalid street type. Please enter a valid type.");
                return;
            }
            Console.WriteLine("City:");
            string city = Console.ReadLine();
            Console.WriteLine("Postcode (1000..9999):");
            int postcode;
            while (!int.TryParse(Console.ReadLine(), out postcode) || postcode < 1000 || postcode > 9999)
            {
                Console.WriteLine("Invalid input. Please enter a valid postcode between 1000 and 9999 inclusive.");
            }

            Console.WriteLine("State (QLD, NSW, VIC, TAS, SA, WA, NT, ACT):");
            string state = Console.ReadLine().ToUpper();

            if (!StateValidation(state))
            {
                Console.WriteLine("Invalid state. Please enter a valid state.");
                return;
            }
         
            string fullAddress = $"{(unitNum.HasValue ? "U" + unitNum.Value.ToString() : "")} {streetNum} {streetName} {streetType}, {city} {state} {postcode}";
            signedInUser.HomeAddress = fullAddress;
            Console.WriteLine($"\nAddress successfully recorded as:\n\t{fullAddress}");
        }

        private bool StreeTypeValidation(string streetType)
        {
            string[] validSuffixes = { "St", "Rd", "Ave", "Blvd", "Dr", "Ln", "Ct", "Pl", "Ter", "Way" };
            return validSuffixes.Contains(streetType);
        }

        private bool StateValidation(string state)
        {
            string[] validStates = { "QLD", "NSW", "VIC", "TAS", "SA", "WA", "NT", "ACT" };
            return validStates.Contains(state.ToUpper());
        }
    }


    public class RegisterUser : IMenuOption
    {
        public void Execute(User signedInUser)
        {
            while (true)
            {
                Console.WriteLine("+-------------------+");
                Console.WriteLine("| Register Customer |");
                Console.WriteLine("+-------------------+\n");
                Console.WriteLine("Name:");
                var name = Console.ReadLine();
                Console.WriteLine("Email address:");
                var email_id = Console.ReadLine();
                Console.WriteLine("Password:");
                var password_id = Console.ReadLine();

                if (!NameValidation(name))
                {
                    Console.WriteLine("Invalid name. Please provide a valid name.\n");
                    continue; 
                }

                if (!EmailValidation(email_id))
                {
                    Console.WriteLine("Invalid email format. Please enter a valid email address.");
                    continue;
                }
                if (!PasswordValidation(password_id))
                {
                    Console.WriteLine("Invalid password. Please provide a valid password.\n");
                    continue;
                }

                // Check for existing email
                bool emailAlreadyExists = false;
                for (int i = 0; i < Program.accUsers.Length; i++)
                {
                    if (Program.accUsers[i] != null && Program.accUsers[i].Email.ToLower() == email_id.ToLower())
                    {
                        Console.WriteLine("Email already exists. Please choose a different email.");
                        emailAlreadyExists = true;
                        break;
                    }
                }

                if (!emailAlreadyExists)
                {
                    if (name != null && email_id != null && password_id != null)
                    {
                         // Find the first available index in the accUsers array
                        int index = Array.IndexOf(Program.accUsers, null);
                        if (index != -1)
                        {
                            int userId = index + 1;
                            Program.accUsers[index] = new User(userId, name, email_id, password_id);
                            Console.WriteLine("\nSuccessfully registered as:");
                            Console.WriteLine($"\t{name ?? "Unknown"} ({email_id ?? "Unknown"}).");
                            Console.WriteLine("\nReturning to the main menu to sign in.\n");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Error: Unable to register. The system is at full capacity.");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please provide valid values for name, email, and password.");
                    }
                }
            }
        }
        private bool NameValidation(string name)
        {
            // Allow letters (A-Za-z), spaces (numeric code 32), dashes (-), and apostrophes (')
            if (!name.All(c => (c >= 65 && c <= 90) || (c >= 97 && c <= 122) || c == 32 || c == '-' || c == '\''))
                return false;

            // The first and last symbols of the name must be letters
            if (!char.IsLetter(name.First()) || !char.IsLetter(name.Last()))
                return false;

            // Any non-letter symbol must appear between two letters
            for (int i = 1; i < name.Length - 1; i++)
            {
                if (!char.IsLetter(name[i]) && (name[i - 1] == ' ' || name[i - 1] == '-' || name[i - 1] == '\'') && char.IsLetter(name[i + 1]))
                    return false;
            }

            return true;
        }

        private bool EmailValidation(string email)
        {

            // Check if the email contains a single @ symbol
            if (email.Count(c => c == '@') != 1)
                return false;

            // Split the email into prefix and suffix
            string[] parts = email.Split('@');
            string prefix = parts[0];
            string suffix = parts[1];

            if (!IsValidPrefix(prefix))
                return false;

            if (!IsValidSuffix(suffix))
                return false;

            return true;
        }

        private bool IsValidPrefix(string prefix)
        {
            // Only allow letters, digits, underscores (_), dashes (-), and dots (.)
            if (!prefix.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.'))
                return false;

            // The last character must not be an underscore, dash, or dot
            if (prefix.EndsWith('_') || prefix.EndsWith('-') || prefix.EndsWith('.'))
                return false;

            return true;
        }

        private bool IsValidSuffix(string suffix)
        {
            // Only allow letters, digits, dashes (-), and dots (.)
            if (!suffix.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '.'))
                return false;

            if (!suffix.Contains('.'))
                return false;

            // Dashes and dots cannot be the first or last character of the suffix
            if (suffix.StartsWith('-') || suffix.StartsWith('.') || suffix.EndsWith('-') || suffix.EndsWith('.'))
                return false;

            // Following the last dot, only letters are permitted
            string[] parts = suffix.Split('.');
            if (parts.Length > 1 && !parts.Last().All(char.IsLetter))
                return false;

            return true;
        }
        private bool PasswordValidation(string password)
        {

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                // Password should not be null, empty, or have less than 8 characters
                return false;
            }
            bool hasUppercase = false;
            bool hasLowercase = false;
            bool hasDigit = false;
            bool hasNonAlphanumeric = false;

            foreach (char character in password)
            {
                if (char.IsUpper(character))
                {
                    hasUppercase = true;
                }
                else if (char.IsLower(character))
                {
                    hasLowercase = true;
                }
                else if (char.IsDigit(character))
                {
                    hasDigit = true;
                }
                else if (!char.IsWhiteSpace(character) && !char.IsLetterOrDigit(character))
                {
                    hasNonAlphanumeric = true;
                }
            }

            return hasUppercase && hasLowercase && hasDigit && hasNonAlphanumeric;
        }
    }
    

    public class SignInUser : IMenuOption
    {
        public void Execute(User signedInUser)
        {
            Console.WriteLine("+------------------+");
            Console.WriteLine("| Customer Sign In |");
            Console.WriteLine("+------------------+");
            bool successful = false;

            while (!successful)
            {
                Console.WriteLine("\nEmail address:");
                var email = Console.ReadLine();
                Console.WriteLine("Password:");
                var password = Console.ReadLine();

                foreach (User user in Program.accUsers)
                {
                    if (email.ToLower() == user.Email.ToLower() && password == user.Password)
                    {
                        Console.WriteLine($"\nSuccessfully signed in as {user.Name} ({user.Email})\n");

                        // If the user is signing in for the first time, ask for the address
                        if (string.IsNullOrEmpty(user.HomeAddress))
                        {
                            new UserAddressMenu().Execute(user);
                            Console.WriteLine("\nContinuing to Client Menu.\n");

                        }
                        successful = true;
                        signedInUser = user;
                        break;
                    }
                }

                if (!successful)
                {
                    Console.WriteLine("Invalid email or password. Please try again.");
                    break;
                }
            }
            ClientMenu.ShowMenu(signedInUser);
        }
    }


    public class ClientMenu
    {
        public static void ShowMenu(User signedInUser)
        {
            Console.WriteLine("+-------------+");
            Console.WriteLine("| Client Menu |");
            Console.WriteLine("+-------------+");
            Console.WriteLine("\nPlease select an option from the following list:");
            Console.WriteLine("1\t\t: Advertise product");
            Console.WriteLine("2\t\t: List my advertised products");
            Console.WriteLine("3\t\t: Search for products to buy");
            Console.WriteLine("4\t\t: Display bids for my products");
            Console.WriteLine("5\t\t: Show my purchases");
            Console.WriteLine("6\t\t: Log out\n");
            Console.Write("? ");

            int option = int.Parse(Console.ReadLine());
            ExecuteMenuOption(option, signedInUser);
        }

        private static void ExecuteMenuOption(int option, User signedInUser)
        {
            IMenuOption menuOption = null;

            switch (option)
            {
                case 1:
                    menuOption = new AdvertiseProduct();
                    break;
                case 2:
                    menuOption = new ProductList(signedInUser);
                    break;
                case 3:
                    menuOption = new SearchProduct();
                    break;
                case 4:
                    menuOption = new DisplayProductBids();
                    break;
                case 5:
                    menuOption = new PurchasedProductsList();
                    break;
                case 6:
                    break;
                default:
                    Console.WriteLine("Invalid option. Please enter a valid choice.");
                    return;
            }

            menuOption?.Execute(signedInUser);
        }
    }

    public class Program
    {
        public static User[] accUsers = new User[100];

        public static void Main(string[] args)
        {
            User signedInUser = null;
            while (true) // Main program loop
            {
                Console.WriteLine("+------------------------------+");
                Console.WriteLine("| Welcome to the Auction House |");
                Console.WriteLine("+------------------------------+\n");
                Console.WriteLine("Please select an option from the following list:\n");
                Console.WriteLine("1\t\t: Register");
                Console.WriteLine("2\t\t: Sign in");
                Console.WriteLine("3\t\t: Exit\n");
                Console.Write("? ");

                int option = int.Parse(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        new RegisterUser().Execute(signedInUser);
                        break;
                    case 2:
                        new SignInUser().Execute(signedInUser);
                        break;
                    case 3:
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please enter a valid choice.");
                        break;
                }
            }
        }
    }
}

