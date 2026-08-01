namespace ShareMeal.Web.Models;

public enum OrganizationType
{
    Restaurant,
    Charity,
    NGO
}

public enum FoodCategory
{
    PreparedMeals,
    Produce,
    Bakery,
    Dairy,
    Other
}

public enum DonationStatus
{
    Available,
    Claimed,
    Collected,
    Cancelled
}

public enum OrganizationStatus
{
    Pending,
    Verified,
    Suspended,
    Rejected
}
