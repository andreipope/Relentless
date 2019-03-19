from .base import CZBTests
class Match_Selection_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.match_selection_page=self.get_match_selection_page()
        self.solo_button=self.get_solo_button()
        self.pvp_button=self.get_pvp_button()
        self.tutorial_button=self.get_tutorial_button()
        self.back_button=self.get_back_button()
    
    def get_match_selection_page(self):
        return self.altdriver.wait_for_element('PlaySelectionPage(Clone)')
    def get_solo_button(self):
        return self.altdriver.wait_for_element(self.match_selection_page.name+'/Button_SoloMode')
    def get_pvp_button(self):
        return self.altdriver.wait_for_element(self.match_selection_page.name+'/Button_PvPMode')
    def get_tutorial_button(self):
        return self.altdriver.wait_for_element(self.match_selection_page.name+'/Button_Tutorial')
    def get_back_button(self):
        return self.altdriver.wait_for_element(self.match_selection_page.name+'/Button_Back')

    def press_solo_button(self):
        self.button_pressed(self.solo_button)
    def press_pvp_button(self):
        self.button_pressed(self.solo_button)
    def press_tutorial_button(self):
        self.button_pressed(self.solo_button)
    def press_back_button(self):
        self.button_pressed(self.solo_button)

