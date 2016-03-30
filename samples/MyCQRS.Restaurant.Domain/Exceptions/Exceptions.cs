using System;

namespace MyCQRS.Restaurant.Domain.Exceptions
{
    public class TabNotOpen : Exception
    {
    }

    public class DrinksNotOutstanding : Exception
    {
    }

    public class FoodNotOutstanding : Exception
    {
    }

    public class FoodNotPrepared : Exception
    {
    }

    public class MustPayEnough : Exception
    {
    }

    public class TabHasUnservedItems : Exception
    {
    }
}