using System;
using System.Linq;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    public class Issue2900
    {
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2900
        public void Override64DepthLevelOnJObjectParse()
        {

            var json = @"
{
  ""level1"": {
    ""level2"": {
      ""level3"": {
        ""level4"": {
          ""level5"": {
            ""level6"": {
              ""level7"": {
                ""level8"": {
                  ""level9"": {
                    ""level10"": {
                      ""level11"": {
                        ""level12"": {
                          ""level13"": {
                            ""level14"": {
                              ""level15"": {
                                ""level16"": {
                                  ""level17"": {
                                    ""level18"": {
                                      ""level19"": {
                                        ""level20"": {
                                          ""level21"": {
                                            ""level22"": {
                                              ""level23"": {
                                                ""level24"": {
                                                  ""level25"": {
                                                    ""level26"": {
                                                      ""level27"": {
                                                        ""level28"": {
                                                          ""level29"": {
                                                            ""level30"": {
                                                              ""level31"": {
                                                                ""level32"": {
                                                                  ""level33"": {
                                                                    ""level34"": {
                                                                      ""level35"": {
                                                                        ""level36"": {
                                                                          ""level37"": {
                                                                            ""level38"": {
                                                                              ""level39"": {
                                                                                ""level40"": {
                                                                                  ""level41"": {
                                                                                    ""level42"": {
                                                                                      ""level43"": {
                                                                                        ""level44"": {
                                                                                          ""level45"": {
                                                                                            ""level46"": {
                                                                                              ""level47"": {
                                                                                                ""level48"": {
                                                                                                  ""level49"": {
                                                                                                    ""level50"": {
                                                                                                      ""level51"": {
                                                                                                        ""level52"": {
                                                                                                          ""level53"": {
                                                                                                            ""level54"": {
                                                                                                              ""level55"": {
                                                                                                                ""level56"": {
                                                                                                                  ""level57"": {
                                                                                                                    ""level58"": {
                                                                                                                      ""level59"": {
                                                                                                                        ""level60"": {
                                                                                                                          ""level61"": {
                                                                                                                            ""level62"": {
                                                                                                                              ""level63"": {
                                                                                                                                ""level64"": {
                                                                                                                                  ""level65"": {
                                                                                                                                    ""data"": ""value""
                                                                                                                                  }
                                                                                                                                }
                                                                                                                              }
                                                                                                                            }
                                                                                                                          }
                                                                                                                        }
                                                                                                                      }
                                                                                                                    }
                                                                                                                  }
                                                                                                                }
                                                                                                              }
                                                                                                            }
                                                                                                          }
                                                                                                        }
                                                                                                      }
                                                                                                    }
                                                                                                  }
                                                                                                }
                                                                                              }
                                                                                            }
                                                                                          }
                                                                                        }
                                                                                      }
                                                                                    }
                                                                                  }
                                                                                }
                                                                              }
                                                                            }
                                                                          }
                                                                        }
                                                                      }
                                                                    }
                                                                  }
                                                                }
                                                              }
                                                            }
                                                          }
                                                        }
                                                      }
                                                    }
                                                  }
                                                }
                                              }
                                            }
                                          }
                                        }
                                      }
                                    }
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
            ";
            
            var settings = new JsonLoadSettings
            {
                MaxDepth = 128
            };
            
            
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
              JObject.Parse(json);
            });
            
            JObject.Parse(json, settings);
            
        }
        
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2900
        public void Override64DepthLevelOnJArrayParse()
        {

            var json = @"
[{
  ""level1"": {
    ""level2"": {
      ""level3"": {
        ""level4"": {
          ""level5"": {
            ""level6"": {
              ""level7"": {
                ""level8"": {
                  ""level9"": {
                    ""level10"": {
                      ""level11"": {
                        ""level12"": {
                          ""level13"": {
                            ""level14"": {
                              ""level15"": {
                                ""level16"": {
                                  ""level17"": {
                                    ""level18"": {
                                      ""level19"": {
                                        ""level20"": {
                                          ""level21"": {
                                            ""level22"": {
                                              ""level23"": {
                                                ""level24"": {
                                                  ""level25"": {
                                                    ""level26"": {
                                                      ""level27"": {
                                                        ""level28"": {
                                                          ""level29"": {
                                                            ""level30"": {
                                                              ""level31"": {
                                                                ""level32"": {
                                                                  ""level33"": {
                                                                    ""level34"": {
                                                                      ""level35"": {
                                                                        ""level36"": {
                                                                          ""level37"": {
                                                                            ""level38"": {
                                                                              ""level39"": {
                                                                                ""level40"": {
                                                                                  ""level41"": {
                                                                                    ""level42"": {
                                                                                      ""level43"": {
                                                                                        ""level44"": {
                                                                                          ""level45"": {
                                                                                            ""level46"": {
                                                                                              ""level47"": {
                                                                                                ""level48"": {
                                                                                                  ""level49"": {
                                                                                                    ""level50"": {
                                                                                                      ""level51"": {
                                                                                                        ""level52"": {
                                                                                                          ""level53"": {
                                                                                                            ""level54"": {
                                                                                                              ""level55"": {
                                                                                                                ""level56"": {
                                                                                                                  ""level57"": {
                                                                                                                    ""level58"": {
                                                                                                                      ""level59"": {
                                                                                                                        ""level60"": {
                                                                                                                          ""level61"": {
                                                                                                                            ""level62"": {
                                                                                                                              ""level63"": {
                                                                                                                                ""level64"": {
                                                                                                                                  ""level65"": {
                                                                                                                                    ""data"": ""value""
                                                                                                                                  }
                                                                                                                                }
                                                                                                                              }
                                                                                                                            }
                                                                                                                          }
                                                                                                                        }
                                                                                                                      }
                                                                                                                    }
                                                                                                                  }
                                                                                                                }
                                                                                                              }
                                                                                                            }
                                                                                                          }
                                                                                                        }
                                                                                                      }
                                                                                                    }
                                                                                                  }
                                                                                                }
                                                                                              }
                                                                                            }
                                                                                          }
                                                                                        }
                                                                                      }
                                                                                    }
                                                                                  }
                                                                                }
                                                                              }
                                                                            }
                                                                          }
                                                                        }
                                                                      }
                                                                    }
                                                                  }
                                                                }
                                                              }
                                                            }
                                                          }
                                                        }
                                                      }
                                                    }
                                                  }
                                                }
                                              }
                                            }
                                          }
                                        }
                                      }
                                    }
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}]
            ";
            
            var settings = new JsonLoadSettings
            {
                MaxDepth = 128
            };
            
            
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
              JArray.Parse(json);
            });
            
          
            JArray.Parse(json, settings);

        }
    }
}