using System;

namespace Sonyakh;

public class SonyakhException(string message) : ApplicationException(message)
{}