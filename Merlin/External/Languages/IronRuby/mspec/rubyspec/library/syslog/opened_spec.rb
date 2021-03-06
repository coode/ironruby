require File.dirname(__FILE__) + '/../../spec_helper'
require 'syslog'

describe "Syslog.opened?" do
  not_supported_on :windows do

    before :each do
      Syslog.opened?.should be_false
    end

    after :each do
      Syslog.opened?.should be_false
    end

    it "returns true if the log is opened" do
      Syslog.open
      Syslog.opened?.should be_true
      Syslog.close
    end
    
    it "returns false otherwise" do
      Syslog.opened?.should be_false
      Syslog.open
      Syslog.close
      Syslog.opened?.should be_false
    end

    it "works inside a block" do
      Syslog.open do |s|
        s.opened?.should be_true
        Syslog.opened?.should be_true
      end
      Syslog.opened?.should be_false
    end
  end
end
