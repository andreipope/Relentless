from base import CZBTests
class Overlord_Selection_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.overlord_selection_page=self.get_overlord_selection_page()
        self.back_button=self.get_back_button()
        self.left_arrow_button=self.get_left_arrow_button()
        self.right_arrow_button=self.get_right_arrow_button()
        self.continue_button=self.get_continue_button()
    
    def get_overlord_selection_page(self):
        return self.altdriver.wait_for_element('OverlordSelectionPage(Clone)')
    def get_back_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Button_Back')
    def get_left_arrow_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Button_LeftArrow')
    def get_right_arrow_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Button_RightArrow')
    def get_continue_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Image_BottomMask/Button_Continue')
    def press_left_arrow(self):
        self.button_pressed(self.left_arrow_button)
    def press_right_arrow(self):
        self.button_pressed(self.right_arrow_button)
    def press_continue(self):
        self.button_pressed(self.continue_button)
    def press_back(self):
        self.button_pressed(self.back_button)
    
    def select_overlord(self,overlord):
        started_overlord_selected=self.altdriver.find_element_where_name_contains('(Selected)')
        started_overlord_selected_name=started_overlord_selected.name.split('(Selected)')[0]
        if started_overlord_selected_name==overlord:
            return started_overlord_selected
        while True:
            self.press_left_arrow()
            overlord_selected=self.altdriver.find_element_where_name_contains('(Selected)')
            overlord_selected_name=overlord_selected.name.split('(Selected)')[0]
            if overlord_selected_name==overlord:
                return overlord_selected
            if overlord_selected_name==started_overlord_selected_name:
                return None


